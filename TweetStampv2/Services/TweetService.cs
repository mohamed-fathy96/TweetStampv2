using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Events;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Tweetinvi.Streaming;
using TweetStampv2.Data;
using TweetStampv2.Models;

namespace TweetStampv2.Services
{
    public class TweetService : ITweetService
    {
        private readonly ITwitterClient twitterClient;
        private readonly IAccountActivityStream accountActivityStream;
        private readonly IConfiguration configuration;
        private readonly TweetContext context;
        private readonly long userId;
        public TweetService(ITwitterClient twitterClient, IAccountActivityRequestHandler handler,
            IConfiguration configuration, TweetContext context)
        {
            this.twitterClient = twitterClient;
            this.configuration = configuration;
            userId = long.Parse(configuration["userId"]);
            this.accountActivityStream = handler.GetAccountActivityStream(userId, "development");
            this.context = context;
            //this.userId = GetDevUserId().Result;
            //this.Subscribe();
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
                //await twitterClient.Messages.PublishMessageAsync("Please send me a valid tweet Url.", senderId);
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
        private string HashTweet(string tweetJson)
        {
            StringBuilder Sb = new();

            using (var hash = SHA256.Create())
            {

                Encoding enc = Encoding.UTF8;

                byte[] result = hash.ComputeHash(enc.GetBytes(tweetJson));
                var stringResult = Convert.ToHexString(result);
                foreach (var b in result)
                {
                    Sb.Append(b.ToString("x2"));
                }
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
            var webhooks = await twitterClient.AccountActivity.GetAccountActivityEnvironmentWebhooksAsync("development");
            if (webhooks.Length > 0)
            {
                await twitterClient.AccountActivity.DeleteAccountActivityWebhookAsync("development", webhooks[0]);
            }
            await twitterClient.AccountActivity.CreateAccountActivityWebhookAsync
                    ("development", "https://6575-197-38-216-196.eu.ngrok.io/Tweet/Webhook");
            await twitterClient.AccountActivity.SubscribeToAccountActivityAsync("development");

            var environmentState = await twitterClient.AccountActivity.GetAccountActivitySubscriptionsAsync("development");
            var userId = long.Parse(environmentState.Subscriptions[0].UserId);

            // This requires applications credentials with a bearer token
            var environments = await twitterClient.AccountActivity.GetAccountActivityWebhookEnvironmentsAsync();

            // the sandbox environment is available within the free tier
            var developmentEnvironment = environments.Single(x => x.Name == "development");

            // registered webhooks url are available in the Webhooks collection
            var registeredWebhooks = developmentEnvironment.Webhooks;

            Trace.WriteLine(environmentState.Subscriptions[0].UserId);
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

            // TODO Get user Profile image, and name, save them to Db with tweet , update TweetModel

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

            var tweetHash = HashTweet(tweetJson);

            tweetObj.Hash = tweetHash;

            await SaveTweetToDb(tweetObj);
        }
    }
}
