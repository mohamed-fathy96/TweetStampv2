﻿using Microsoft.EntityFrameworkCore;
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
        private readonly IAccountActivityRequestHandler handler;
        private readonly IAccountActivityStream accountActivityStream;
        private readonly IConfiguration configuration;
        private readonly long userId;
        private string rawTweetUrl;
        public TweetService(ITwitterClient twitterClient, IAccountActivityRequestHandler handler, 
            IConfiguration configuration)
        {
            this.twitterClient = twitterClient;
            this.handler = handler;           
            this.configuration = configuration;
            userId = long.Parse(configuration["userId"]);
            this.accountActivityStream = handler.GetAccountActivityStream(userId, "development");
            //this.userId = GetUserId().Result;
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

                rawTweetUrl = tweetUrl.Split('?')[0];

                var tweetId = long.Parse(tweetUrl.Split("status/")[1].Split('?')[0]);

                var tweet = await twitterClient.Tweets.GetTweetAsync(tweetId);

                var tweetObj = CreateTweet(tweet);

                var tweetJson = Tweet.ToJson(tweetObj);

                var tweetHash = HashTweet(tweetJson);

                tweetObj.Hash = tweetHash;

                await SaveTweetToDb(tweetObj);
                
                await twitterClient.Messages.PublishMessageAsync($"Here's your stringified tweet's hash: " +
                    $"{tweetHash}", senderId);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                await twitterClient.Messages.PublishMessageAsync("Please send me a valid tweet Url.", senderId);
            }

        }

        public async Task StampTweetMention(TweetCreatedEvent e)
        {
            var messageJson = e.Json;
            JObject jsonObj = JObject.Parse(messageJson);
            var userTweetId = (string)jsonObj["tweet_create_events"][0]["id"];
            var userName = (string)jsonObj["tweet_create_events"][0]["user"]["screen_name"];

            try
            {
                var tweetUrl = (string)jsonObj["tweet_create_events"][0]["in_reply_to_status_id_str"];

                var tweetId = long.Parse(tweetUrl);

                var tweet = await twitterClient.Tweets.GetTweetAsync(tweetId);

                var tweetObj = CreateTweet(tweet);

                var tweetJson = Tweet.ToJson(tweetObj);

                var tweetHash = HashTweet(tweetJson);

                tweetObj.Hash = tweetHash;

                await SaveTweetToDb(tweetObj);

                await twitterClient.Tweets.PublishTweetAsync(
                    new PublishTweetParameters($"@{userName} here's your stringified tweet's hash: " +
                    $"{tweetHash}")
                    {
                        InReplyToTweetId = long.Parse(userTweetId)
                    });
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                await twitterClient.Messages.PublishMessageAsync("If you would like to stamp a tweet," +
                "please make sure to include the word 'stamp' in your reply.", long.Parse(userTweetId));
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
                FullText = tweet.FullText,
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
                    ("development", "https://4c78-197-38-146-68.eu.ngrok.io/Tweet/Webhook");
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

        private async Task<long> GetUserId()
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

            using (var context = new TweetContext(contextOptions))
            {
                var tweetModel = new TweetModel()
                {
                    Id = long.Parse(tweet.Id),
                    Url = rawTweetUrl,
                    Text = tweet.FullText,
                    UserId = tweet.User.Id,
                    CreatedAt = tweet.CreatedAt,
                    UserScreenName = tweet.User.ScreenName,
                    Json = tweet.Json,
                    Hash = tweet.Hash
                };
                context.Tweets.Add(tweetModel);
                await context.SaveChangesAsync();
            }
        }
    }
}
