namespace TweetStampv2.DTOs
{
    public class Notification
    {
        public int? Currency { get; set; }
        public int? NotificationType { get; set; }
        public string Target { get; set; }

        public Notification(int? currency = null, int? notificationType = null, string target = null)
        {
            Currency = currency;
            NotificationType = notificationType;
            Target = target;
        }
    }
}