using System.ComponentModel.DataAnnotations;

namespace TweetStampv2.Models
{
    public class TweetModel
    {
        [Required]
        public long Id { get; set; }
        public string Url { get; set; }
        public string Text { get; set; }
        public string UserId { get; set; }
        public string CreatedAt { get; set; }
        public string UserScreenName { get; set; }
        public string Json { get; set; }
        public string Hash { get; set; }
    }
}
