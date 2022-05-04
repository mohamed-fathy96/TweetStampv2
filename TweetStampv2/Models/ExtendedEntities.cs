using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using Tweetinvi.Models.Entities;

namespace TweetStampv2.Models
{
    [Owned]
    public class ExtendedEntities
    {
        [JsonProperty("media")]
        public List<TweetMedia> Media { get; set; }
        public ExtendedEntities(List<IMediaEntity> list)
        {
            Media = new List<TweetMedia>();

            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    Media.Add(new TweetMedia(item.Indices)
                    {
                        Id = (long)item.Id,
                        IdStr = item.IdStr,
                        MediaUrl = item.MediaURL,
                        MediaUrlHttps = item.MediaURLHttps,
                        Url = item.URL,
                        DisplayUrl = item.DisplayURL,
                        ExpandedUrl = item.ExpandedURL,
                        Type = item.MediaType
                    });
                }
            }
        }
    }
}
