using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using Tweetinvi.Models.Entities;

namespace TweetStampv2.Models
{
    [Owned]
    public class Entities
    {
        [JsonProperty("urls")]

        public List<string> Urls;
        public Entities(ICollection<IUrlEntity> Urls)
        {
            this.Urls = new List<string>();

            if (Urls.Count > 0)
            {
                foreach (var url in Urls)
                {
                    this.Urls.Add(url.URL);
                }
            }
        }
    }
}
