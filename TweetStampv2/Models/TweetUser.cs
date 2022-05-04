using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace TweetStampv2.Models
{
    public class TweetUser
    {       
        [JsonProperty("id_str")]
        public string Id { get; set; }
        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }
    }
}
