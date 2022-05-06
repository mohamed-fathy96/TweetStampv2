using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using tr.gov.tubitak.uekae.esya.api.asn.cms;
using tr.gov.tubitak.uekae.esya.api.asn.pkixtsp;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.certificate.validation.policy;
using tr.gov.tubitak.uekae.esya.api.cmssignature.signature;
using tr.gov.tubitak.uekae.esya.api.cmssignature.validation;
using tr.gov.tubitak.uekae.esya.api.common.util;
using tr.gov.tubitak.uekae.esya.api.crypto.alg;
using tr.gov.tubitak.uekae.esya.api.crypto.util;
using tr.gov.tubitak.uekae.esya.api.infra.tsclient;
using Tweetinvi;
using Tweetinvi.Events;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Tweetinvi.Streaming;
using TweetStampv2.Data;
using TweetStampv2.m3api.tr_TR;
using TweetStampv2.Models;

namespace TweetStampv2.Services
{
    public class TweetService : ITweetService
    {
        private readonly ITwitterClient twitterClient;
        private readonly IAccountActivityStream accountActivityStream;
        private readonly IConfiguration configuration;
        private readonly TweetContext context;
        private static IWebhook[] webhooks;
        public TweetService(ITwitterClient twitterClient, IAccountActivityRequestHandler handler,
            IConfiguration configuration, TweetContext context)
        {
            this.twitterClient = twitterClient;
            this.configuration = configuration;
            var userId = long.Parse(configuration["userId"]);
            this.accountActivityStream = handler.GetAccountActivityStream(userId, configuration["envName"]);
            this.context = context;

            // TODO: Uncomment this before deploying

            //if (webhooks == null)
            //{
            //    this.Subscribe();
            //}
        }

        public void WebhookSubscribe(EventHandler<MessageReceivedEvent> Webhook,
            EventHandler<TweetCreatedEvent> ReplyWebhook)
        {
            if (accountActivityStream.MessageReceived == null)
                accountActivityStream.MessageReceived += Webhook;

            if (accountActivityStream.TweetCreated == null)
                accountActivityStream.TweetCreated += ReplyWebhook;
        }
        public async Task StampTweetDM(MessageReceivedEvent e)
        {
            var messageJson = e.Json;
            JObject jsonObj = JObject.Parse(messageJson);
            var senderId = long.Parse((string)jsonObj["direct_message_events"][0]["message_create"]["sender_id"]);

            try
            {
                var tweetUrl = (string)jsonObj["direct_message_events"][0]["message_create"]["message_data"]
                ["entities"]["urls"][0]["expanded_url"];

                var tweetId = long.Parse(tweetUrl.Split("status/")[1].Split('?')[0]);

                if (!await TweetExists(tweetId))
                {
                    await CreateAndSaveTweetAsync(tweetId);
                    await twitterClient.Messages.PublishMessageAsync($"{configuration["hostUrl"]}/{tweetId}", senderId);
                }
                else
                {
                    await twitterClient.Messages.PublishMessageAsync($"{configuration["hostUrl"]}/{tweetId}", senderId);
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);              
            }

        }
        public async Task StampTweetMention(TweetCreatedEvent e)
        {
            var messageJson = e.Json;
            JObject jsonObj = JObject.Parse(messageJson);
            if (((string)jsonObj["tweet_create_events"][0]["text"]).ToLower().Contains("stamp"))
            {
                var userTweetId = (string)jsonObj["tweet_create_events"][0]["id"];
                var userName = (string)jsonObj["tweet_create_events"][0]["user"]["screen_name"];

                try
                {
                    var tweetUrl = (string)jsonObj["tweet_create_events"][0]["in_reply_to_status_id_str"];

                    var tweetId = long.Parse(tweetUrl);
                    if (!await TweetExists(tweetId))
                    {
                        await CreateAndSaveTweetAsync(tweetId);
                        await twitterClient.Tweets.PublishTweetAsync(
                        new PublishTweetParameters($"@{userName} {configuration["hostUrl"]}/{tweetId}")
                        {
                            InReplyToTweetId = long.Parse(userTweetId)
                        });
                    }
                    else
                    {
                        await twitterClient.Tweets.PublishTweetAsync(
                        new PublishTweetParameters($"@{userName} {configuration["hostUrl"]}/{tweetId}")
                        {
                            InReplyToTweetId = long.Parse(userTweetId)
                        });
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    //await twitterClient.Messages.PublishMessageAsync("If you would like to stamp a tweet," +
                    //"please make sure to include the word 'stamp' in your reply.", long.Parse(userTweetId));
                }
            }
        }
        private string ByteArrToString(byte[] arr)
        {
            StringBuilder Sb = new();

            foreach (var b in arr)
            {
                Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }
        private Tweet CreateTweet(ITweet tweet)
        {
            return new Tweet()
            {
                Id = tweet.IdStr,
                FullText = tweet.Text,
                Source = tweet.Source,
                CreatedAt = tweet.CreatedAt.ToString(),
                User = new TweetUser()
                {
                    Id = tweet.CreatedBy.IdStr,
                    ScreenName = tweet.CreatedBy.ScreenName
                },
                Entities = new Entities(tweet.Entities.Urls),
                ExtendedEntities = new ExtendedEntities(tweet.Entities.Medias)
            };
        }
        private async void Subscribe()
        {
            webhooks = await twitterClient.AccountActivity.GetAccountActivityEnvironmentWebhooksAsync("development");

            //TODO: Change to actual hosting domain
            await twitterClient.AccountActivity.CreateAccountActivityWebhookAsync
                    (configuration["envName"], "https://8c9d-197-38-209-20.eu.ngrok.io/Tweet/Webhook");
            await twitterClient.AccountActivity.SubscribeToAccountActivityAsync(configuration["envName"]);

            //var environmentState = await twitterClient.AccountActivity.GetAccountActivitySubscriptionsAsync(configuration["envName"]);
            //var userId = long.Parse(environmentState.Subscriptions[0].UserId);

            //// This requires applications credentials with a bearer token
            //var environments = await twitterClient.AccountActivity.GetAccountActivityWebhookEnvironmentsAsync();

            //// the sandbox environment is available within the free tier
            //var developmentEnvironment = environments.Single(x => x.Name == "development");

            //// registered webhooks url are available in the Webhooks collection
            //var registeredWebhooks = developmentEnvironment.Webhooks;

        }
        private async Task<long> GetDevUserId()
        {
            var environmentState = await twitterClient.AccountActivity.GetAccountActivitySubscriptionsAsync("development");
            return long.Parse(environmentState.Subscriptions[0].UserId);
        }
        private async Task SaveTweetToDb(Tweet tweet)
        {
            var contextOptions = new DbContextOptionsBuilder<TweetContext>()
                .UseSqlServer(configuration["connectionString"])
                .Options;

            var user = await twitterClient.Users.GetUserAsync(long.Parse(tweet.User.Id));
            var userProfileImgUrl = user.ProfileImageUrl;
            var userFullName = user.Name;

            using (var context = new TweetContext(contextOptions))
            {
                var embeddedTweet = await twitterClient.Tweets.GetOEmbedTweetAsync(long.Parse(tweet.Id));

                var tweetModel = new TweetModel()
                {
                    Id = long.Parse(tweet.Id),
                    Url = embeddedTweet.URL,
                    Text = tweet.FullText,
                    UserId = tweet.User.Id,
                    CreatedAt = tweet.CreatedAt,
                    UserScreenName = tweet.User.ScreenName,
                    UserFullName = userFullName,
                    UserProfileImgUrl = userProfileImgUrl,
                    EmbbededTweetHTML = embeddedTweet.HTML,
                    ValidationDescription = tweet.ValidationDescription,
                    TimeStampInfo = tweet.TimeStampInfo,
                    TsByteArr = tweet.TsByteArr,
                    Json = tweet.Json,
                    Hash = tweet.Hash
                };

                // TODO: Achieve this logic in a different way
                switch (tweet.ExtendedEntities.Media.Count)
                {
                    case 1:
                        tweetModel.MediaUrl1 = tweet.ExtendedEntities.Media[0].MediaUrl;
                        break;
                    case 2:
                        tweetModel.MediaUrl1 = tweet.ExtendedEntities.Media[0].MediaUrl;
                        tweetModel.MediaUrl2 = tweet.ExtendedEntities.Media[1].MediaUrl;
                        break;
                    case 3:
                        tweetModel.MediaUrl1 = tweet.ExtendedEntities.Media[0].MediaUrl;
                        tweetModel.MediaUrl2 = tweet.ExtendedEntities.Media[1].MediaUrl;
                        tweetModel.MediaUrl3 = tweet.ExtendedEntities.Media[3].MediaUrl;
                        break;
                    case 4:
                        tweetModel.MediaUrl1 = tweet.ExtendedEntities.Media[0].MediaUrl;
                        tweetModel.MediaUrl2 = tweet.ExtendedEntities.Media[1].MediaUrl;
                        tweetModel.MediaUrl3 = tweet.ExtendedEntities.Media[2].MediaUrl;
                        tweetModel.MediaUrl4 = tweet.ExtendedEntities.Media[3].MediaUrl;
                        break;
                    default:
                        break;
                }
                context.Tweets.Add(tweetModel);
                await context.SaveChangesAsync();
            }
        }
        public async Task<TweetModel> GetTweetByIdAsync(long id)
        {
            return await context.Tweets.FirstOrDefaultAsync(t => t.Id == id);
        }
        private async Task<bool> TweetExists(long tweetId)
        {
            var contextOptions = new DbContextOptionsBuilder<TweetContext>()
            .UseSqlServer(configuration["connectionString"])
            .Options;
            using (var context = new TweetContext(contextOptions))
            {
                return await context.Tweets.AnyAsync(t => t.Id == tweetId);
            }
        }
        private async Task CreateAndSaveTweetAsync(long tweetId)
        {
            var tweet = await twitterClient.Tweets.GetTweetAsync(tweetId);

            var tweetObj = CreateTweet(tweet);

            var tweetJson = Tweet.ToJson(tweetObj);

            tweetObj = StampTweet(tweetJson, tweetObj);

            await SaveTweetToDb(tweetObj);
        }
        public Tweet StampTweet(string tweetJson, Tweet tweet)
        {
            byte[] data = Encoding.UTF8.GetBytes(tweetJson);
            byte[] digest = DigestUtil.digest(DigestAlg.SHA256, data);

            tweet.Hash = ByteArrToString(digest);

            TSClient tsClient = new TSClient();
            TSSettings settings = new TSSettings("http://tzd.kamusm.gov.tr", int.Parse(configuration["stampUserId"]),
            configuration["stampUserPassword"], DigestAlg.SHA256);
            ETimeStampResponse response = tsClient.timestamp(digest, settings);
            byte[] tsBytes = response.getContentInfo().getEncoded();
            tweet.TsByteArr = tsBytes;

            ////////////////////////////

            EContentInfo ci = new(tsBytes);
            ESignedData sd = new(ci.getContent());
            ETSTInfo tstInfo = new(sd.getEncapsulatedContentInfo().getContent());

            tweet.TimeStampInfo = ByteArrToString(tstInfo.getBytes());

            byte[] digestInTS = tstInfo.getHashedMessage();

            DigestAlg digestAlg = DigestAlg.fromAlgorithmIdentifier(tstInfo.getHashAlgorithm());

            byte[] actualDigest = DigestUtil.digest(digestAlg, data);
            if (!Arrays.AreEqual(digest, digestInTS))
                Trace.WriteLine("Hashes do not match");

            ////////////////////////

            ////var policyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"m3api/certval-policy.xml");
            //params_["P_VALIDATE_CERTIFICATE_BEFORE_SIGNING"] = false;
            var params_ = new Dictionary<string, object>();
            var POLICY_FILE = Path.Combine("m3api", "certval-policy.xml");
            //var policyFilePath = @"D:\ITI\Freelancer\TweetStamp Project\TweetStampv2\TweetStampv2\TweetStampv2\m3api\certval-policy.xml";
            //ValidationPolicy policy = PolicyReader.readValidationPolicy(new FileStream(policyFilePath, FileMode.Open, FileAccess.Read));
            params_["P_VALIDATE_TIMESTAMP_WHILE_SIGNING"] = true;
            params_["P_TSS_INFO"] = settings;
            params_["P_CERT_VALIDATION_POLICY"] = TestConstants.GetPolicy(POLICY_FILE);
            SignedDataValidation sdv = new();
            SignedDataValidationResult sdvr = sdv.verify(tsBytes, params_);
            tweet.ValidationDescription = sdvr.ToString();
            return tweet;

            //if (sdvr.getSDStatus() != SignedData_Status.ALL_VALID)
            //    Trace.WriteLine("Failed to verify timestamp");

            ///////////////////

        }
        //private static ValidationPolicy TestConstants()
        //{
        //    string dir = Directory.GetCurrentDirectory();

        //    var LICENCE_FILE = Path.Combine("m3api","lisans", "lisans.xml");

        //    setLicence(LICENCE_FILE);

        //    var POLICY_FILE = Path.Combine("m3api", "certval-policy-malimuhur.xml");
        //    return PolicyReader.readValidationPolicy(new FileStream(POLICY_FILE, FileMode.Open, FileAccess.Read));
        //}

        //private static void setLicence(string LICENCE_FILE)
        //{
        //    using (Stream license = new FileStream(LICENCE_FILE, FileMode.Open, FileAccess.Read))
        //    {
        //        LicenseUtil.setLicenseXml(license);
        //    }
        //}
    }

}
