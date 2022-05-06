using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace TweetStampv2.Models
{
    public class Tweet
    {
        [JsonProperty("id_str")]
        [Key]
        public string Id { get; set; }

        [JsonProperty("full_text")]
        public string FullText { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("user")]       
        public TweetUser User { get; set; }

        [JsonProperty("entities")]
        public Entities Entities { get; set; }

        [JsonProperty("extended_entities")]
        public ExtendedEntities ExtendedEntities { get; set; }
        public string Json { get; set; }      
        public string Hash { get; set; }
        public string ValidationDescription { get; set; }
        public string TimeStampInfo { get; set; }
        public byte[] TsByteArr { get; set; }

        public static string ToJson(Tweet t)
        {
            t.Json = JsonConvert.SerializeObject(t,
                    Formatting.None, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
            return t.Json;
        }

    }
}
