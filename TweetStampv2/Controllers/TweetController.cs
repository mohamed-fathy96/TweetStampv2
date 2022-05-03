using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography;
using System.Text;
using Tweetinvi;
using Tweetinvi.Events;
using Tweetinvi.Models;
using System.Diagnostics;
using RestSharp;
using TweetStampv2.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Tweetinvi.Parameters;

namespace TweetStamp.Controllers
{
    public class TweetController : Controller
    {
        private readonly ITwitterClient twitterClient;
        private readonly IAccountActivityRequestHandler handler;

        public TweetController(ITwitterClient client, IAccountActivityRequestHandler handler)
        {
            this.twitterClient = client;
            this.handler = handler;
        }
        public void Index()
        {
            // var webhooks = await twitterClient.AccountActivity.GetAccountActivityEnvironmentWebhooksAsync("development");
            // if (webhooks.Length > 0)
            // {
            //     await twitterClient.AccountActivity.DeleteAccountActivityWebhookAsync("development", webhooks[0]);
            // }

            // await twitterClient.AccountActivity.CreateAccountActivityWebhookAsync
            //     ("development", "https://383e-197-38-226-78.eu.ngrok.io/Tweet/Webhook");
            //await twitterClient.AccountActivity.SubscribeToAccountActivityAsync("development");
            //var environmentState = await twitterClient.AccountActivity.GetAccountActivitySubscriptionsAsync("development");
            //var userId = long.Parse(environmentState.Subscriptions[0].UserId);

            // // This requires applications credentials with a bearer token
            // var environments = await twitterClient.AccountActivity.GetAccountActivityWebhookEnvironmentsAsync();

            // // the sandbox environment is available within the free tier
            // var developmentEnvironment = environments.Single(x => x.Name == "development");

            // // registered webhooks url are available in the Webhooks collection
            // var registeredWebhooks = developmentEnvironment.Webhooks;

            var accountActivityStream = handler.GetAccountActivityStream(1519700194912358400, "development");

            //Trace.WriteLine(environmentState.Subscriptions[0].UserId);
            accountActivityStream.MessageReceived += Webhook;
            accountActivityStream.TweetCreated += ReplyWebhook;
        }

        [HttpPost]
        public async void Webhook(object sender, MessageReceivedEvent e)
        {
            var messageJson = e.Json;
            JObject jsonObj = JObject.Parse(messageJson);
            var tweetUrl = (string)jsonObj["direct_message_events"][0]["message_create"]["message_data"]
                ["entities"]["urls"][0]["expanded_url"];
            var tweetId = long.Parse(tweetUrl.Split("status/")[1].Split('?')[0]);
            var tweet = await twitterClient.Tweets.GetTweetAsync(tweetId);

            var tweetJson = JsonConvert.SerializeObject(tweet.TweetDTO,
                Formatting.None, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });


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

            var tweetHash = Sb.ToString();

            var senderId = long.Parse((string)jsonObj["direct_message_events"][0]["message_create"]["sender_id"]);

            await twitterClient.Messages.PublishMessageAsync($"Here's your stringified tweet's hash: " +
                $"{tweetHash}", senderId);

            //var restClient = new RestClient("https://proofofexistence.com");
            //var restRequest = new RestRequest("api/v1/register", Method.Post);
            //restRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            //restRequest.AddHeader("Accept", "application/json");
            //restRequest.AddBody(new { d = tweetHash });

            //var restResponse = await restClient.PostAsync(restRequest);

        }

        [HttpPost]
        public async void ReplyWebhook(object sender, TweetCreatedEvent e)
        {
            var messageJson = e.Json;
            JObject jsonObj = JObject.Parse(messageJson);
            
            if (((string)jsonObj["tweet_create_events"][0]["text"]).ToLower().Contains("stamp"))
            {
                var tweetUrl = (string)jsonObj["tweet_create_events"][0]["in_reply_to_status_id_str"];
                
                if (long.TryParse(tweetUrl, out long tweetId))
                {
                    var tweet = await twitterClient.Tweets.GetTweetAsync(tweetId);

                    var tweetJson = JsonConvert.SerializeObject(tweet.TweetDTO,
                        Formatting.None, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });


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
                    var tweetHash = Sb.ToString();
                    var userName = (string)jsonObj["tweet_create_events"][0]["user"]["screen_name"];
                    var userTweetId = (string)jsonObj["tweet_create_events"][0]["id"];
                    var reply = await twitterClient.Tweets.PublishTweetAsync(
                        new PublishTweetParameters($"@{userName} here's your stringified tweet's hash: " +
                        $"{tweetHash}")
                        {
                            InReplyToTweetId = long.Parse(userTweetId)
                        });
                    
                }
            }
        }

    }
}
