using System;
using System.Threading.Tasks;
using Tweetinvi.Events;
using TweetStampv2.Models;

namespace TweetStampv2.Services
{
    public interface ITweetService
    {
        public void WebhookSubscribe(EventHandler<MessageReceivedEvent> Webhook,
            EventHandler<TweetCreatedEvent> ReplyWebhook);
        public Task StampTweetDM(MessageReceivedEvent e);
        public Task StampTweetMention(TweetCreatedEvent e);
    }
}
