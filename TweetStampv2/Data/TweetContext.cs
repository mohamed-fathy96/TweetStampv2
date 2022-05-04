using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TweetStampv2.Models;

namespace TweetStampv2.Data
{
    public class TweetContext : DbContext
    {
        public TweetContext(DbContextOptions<TweetContext> options) : base(options)
        {
            
        }
        public virtual DbSet<TweetModel> Tweets { get; set; }
    }
}
