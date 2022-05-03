using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Plugins.Add<AspNetPlugin>();
            var credentials = new TwitterCredentials("0dHtT2WrmeA3HEzYYzDe7Pqwv",
                "XPFpwV5kKeJHF2qfhnEj36HtK4HTRJocs7JovoELsKndnlATEi",
                "1519700194912358400-JZjqud4Z6HefWCXyorAsVkRDyNcpIn",
                "KjOJPoWVMJLNf6P1o0cuSXLrSeLbjwMAidP3j0FsVWR9s")
            {
                BearerToken = "AAAAAAAAAAAAAAAAAAAAAIVCcAEAAAAAbUTLXryvf%2FqCNK0IaAxKBn25fcU%3DH2vc8WbCNE99AgoiQP3ar2Cim9z1UpXVk9DIXDBGamOL9sZcY3"
            };

            WebhookClient = new TwitterClient(credentials);
            AccountActivityRequestHandler = WebhookClient.AccountActivity.CreateRequestHandler();
            services.AddControllersWithViews();
            services.AddSingleton<ITwitterClient>(WebhookClient);
            services.AddSingleton<IAccountActivityRequestHandler>(AccountActivityRequestHandler);
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
