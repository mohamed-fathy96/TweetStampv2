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
using TweetStampv2.Services;
using System.Threading.Tasks;
using TweetStampv2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TweetStampv2.Models;

namespace TweetStamp.Controllers
{
    public class TweetController : Controller
    {
        private readonly ITweetService tweetService;
      
        public TweetController(ITweetService tweetService)//, 
            //TweetContext context)
        {
            this.tweetService = tweetService;
            //this.context = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            //var accountActivityStream = handler.GetAccountActivityStream(1519700194912358400, "development");

            //accountActivityStream.MessageReceived += Webhook;
            //accountActivityStream.TweetCreated += ReplyWebhook;

            tweetService.WebhookSubscribe(Webhook, ReplyWebhook);
            return View();
        }

        [HttpGet]
        [Route("/{id}")]
        public async Task<ActionResult<TweetModel>> GetTweetById(long id)
        {
            //var tweet = context.Tweets.FirstOrDefault(t => t.Id == id);
            if (id == 0)
                return RedirectToAction("Index");

            var tweet = await tweetService.GetTweetByIdAsync(id);

            if(tweet != null)
                return View(tweet);
            return RedirectToAction("TweetNotFound");

        }

        [Route("/about")]
        public IActionResult About()
        {
            return View();
        }
        [Route("/notfound")]
        public IActionResult TweetNotFound()
        {
            return View();
        }

        [HttpPost]
        public void Webhook(object sender, MessageReceivedEvent e)
        {
            tweetService.StampTweetDM(e).Wait();

            //var messageJson = e.Json;
            //JObject jsonObj = JObject.Parse(messageJson);
            //var tweetUrl = (string)jsonObj["direct_message_events"][0]["message_create"]["message_data"]
            //    ["entities"]["urls"][0]["expanded_url"];
            //var tweetId = long.Parse(tweetUrl.Split("status/")[1].Split('?')[0]);
            //var tweet = await twitterClient.Tweets.GetTweetAsync(tweetId);

            //var tweetJson = JsonConvert.SerializeObject(tweet.TweetDTO,
            //    Formatting.None, new JsonSerializerSettings()
            //    {
            //        NullValueHandling = NullValueHandling.Ignore
            //    });


            //StringBuilder Sb = new();

            //using (var hash = SHA256.Create())
            //{

            //    Encoding enc = Encoding.UTF8;

            //    byte[] result = hash.ComputeHash(enc.GetBytes(tweetJson));
            //    var stringResult = Convert.ToHexString(result);
            //    foreach (var b in result)
            //    {
            //        Sb.Append(b.ToString("x2"));
            //    }
            //}

            //var tweetHash = Sb.ToString();

            //var senderId = long.Parse((string)jsonObj["direct_message_events"][0]["message_create"]["sender_id"]);

            //await twitterClient.Messages.PublishMessageAsync($"Here's your stringified tweet's hash: " +
            //    $"{tweetHash}", senderId);

            //var restClient = new RestClient("https://proofofexistence.com");
            //var restRequest = new RestRequest("api/v1/register", Method.Post);
            //restRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            //restRequest.AddHeader("Accept", "application/json");
            //restRequest.AddBody(new { d = tweetHash });

            //var restResponse = await restClient.PostAsync(restRequest);

        }

        [HttpPost]
        public void ReplyWebhook(object sender, TweetCreatedEvent e)
        {
            tweetService.StampTweetMention(e).Wait();
            //var messageJson = e.Json;
            //JObject jsonObj = JObject.Parse(messageJson);

            //if (((string)jsonObj["tweet_create_events"][0]["text"]).ToLower().Contains("stamp"))
            //{
            //    var tweetUrl = (string)jsonObj["tweet_create_events"][0]["in_reply_to_status_id_str"];

            //    if (long.TryParse(tweetUrl, out long tweetId))
            //    {
            //        var tweet = await twitterClient.Tweets.GetTweetAsync(tweetId);

            //        var tweetJson = JsonConvert.SerializeObject(tweet.TweetDTO,
            //            Formatting.None, new JsonSerializerSettings()
            //            {
            //                NullValueHandling = NullValueHandling.Ignore
            //            });


            //        StringBuilder Sb = new();

            //        using (var hash = SHA256.Create())
            //        {

            //            Encoding enc = Encoding.UTF8;

            //            byte[] result = hash.ComputeHash(enc.GetBytes(tweetJson));
            //            var stringResult = Convert.ToHexString(result);
            //            foreach (var b in result)
            //            {
            //                Sb.Append(b.ToString("x2"));
            //            }
            //        }
            //        var tweetHash = Sb.ToString();
            //        var userName = (string)jsonObj["tweet_create_events"][0]["user"]["screen_name"];
            //        var userTweetId = (string)jsonObj["tweet_create_events"][0]["id"];
            //        var reply = await twitterClient.Tweets.PublishTweetAsync(
            //            new PublishTweetParameters($"@{userName} here's your stringified tweet's hash: " +
            //            $"{tweetHash}")
            //            {
            //                InReplyToTweetId = long.Parse(userTweetId)
            //            });

            //    }
            //}
        }


    }
}
