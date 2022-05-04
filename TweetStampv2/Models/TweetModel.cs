using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TweetStampv2.Models
{
	public class TweetModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public string Url { get; set; }
        public string Text { get; set; }
        public string UserId { get; set; }
        public string CreatedAt { get; set; }
        public string UserScreenName { get; set; }
        public string UserFullName { get; set; }
        public string UserProfileImgUrl { get; set; }
        public string EmbbededTweetHTML { get; set; }
        public string MediaUrl1 { get; set; }
        public string MediaUrl2 { get; set; }
        public string MediaUrl3 { get; set; }
        public string MediaUrl4 { get; set; }
        public string Json { get; set; }
        public string Hash { get; set; }
    }
}
