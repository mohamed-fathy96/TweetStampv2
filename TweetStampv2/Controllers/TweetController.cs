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

namespace TweetStamp.Controllers
{
    public class TweetController : Controller
    {
        private readonly ITwitterClient twitterClient;
        private readonly IAccountActivityRequestHandler handler;

        public TweetController(ITwitterClient client , IAccountActivityRequestHandler handler)
        {
            this.twitterClient = client;
            this.handler = handler;          
        }
        public void Index()
        {
           //await client.AccountActivity.CreateAccountActivityWebhookAsync
           //     ("development", "https://3d5b-197-38-251-132.eu.ngrok.io/Tweet/Webhook");
           //await client.AccountActivity.SubscribeToAccountActivityAsync("development");
           //var environmentState = await client.AccountActivity.GetAccountActivitySubscriptionsAsync("development");
           //var userId = long.Parse(environmentState.Subscriptions[0].UserId);
            // This requires applications credentials with a bearer token
            //var environments = await client.AccountActivity.GetAccountActivityWebhookEnvironmentsAsync();

            // the sandbox environment is available within the free tier
            //var developmentEnvironment = environments.Single(x => x.Name == "development");

            // registered webhooks url are available in the Webhooks collection
            //var registeredWebhooks = developmentEnvironment.Webhooks;

            var accountActivityStream = handler.GetAccountActivityStream(1519700194912358400, "development");

            //Trace.WriteLine(environmentState.Subscriptions[0].UserId);
            accountActivityStream.MessageReceived += Webhook;
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

            var restClient = new RestClient("https://api.originstamp.com");
            var restRequest = new RestRequest("v4/timestamp/create", Method.Post);
            restRequest.AddHeader("Content-Type", "application/json");
            restRequest.AddHeader("Accept", "application/json");
            restRequest.AddHeader("Authorization", "182700ca-cc43-4203-a08a-f250828fc979");

            var reqBody = new TimeStampRequestDTO(null, tweetHash, null);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddBody(reqBody);
            var restResponse = await restClient.PostAsync(restRequest);
            var restDto = JsonConvert.DeserializeObject<TimeStampResponseDTO>(restResponse.Content);
            Trace.WriteLine("hello");

        }

    }
}
