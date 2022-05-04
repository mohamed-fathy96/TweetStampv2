using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.AspNet;
using Tweetinvi.Models;
using TweetStampv2.Data;
using TweetStampv2.Services;

namespace TweetStampv2
{
    public class Startup
    {
        public static ITwitterClient WebhookClient { get; set; }
        public static IAccountActivityRequestHandler AccountActivityRequestHandler { get; set; }
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            Plugins.Add<AspNetPlugin>();           
            var credentials = new TwitterCredentials(Configuration["consumerKey"],
                Configuration["consumerSecret"],
                Configuration["accessToken"],
                Configuration["accessTokenSecret"])
            {
                BearerToken = Configuration["bearerToken"] 
            };

            WebhookClient = new TwitterClient(credentials);
            AccountActivityRequestHandler = WebhookClient.AccountActivity.CreateRequestHandler();
            services.AddControllersWithViews();
            services.AddTransient<ITweetService, TweetService>();
            services.AddSingleton(WebhookClient);
            services.AddSingleton(AccountActivityRequestHandler);
            services.AddDbContext<TweetContext>(
                options => options.UseSqlServer(Configuration["connectionString"]));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var config = new WebhookMiddlewareConfiguration(AccountActivityRequestHandler);

            app.UseTweetinviWebhooks(config);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Tweet}/{action=Index}/{id?}");
            });
        }
    }
}
