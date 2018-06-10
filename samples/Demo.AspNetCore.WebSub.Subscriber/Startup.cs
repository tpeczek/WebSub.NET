using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Lib.AspNetCore.ServerSentEvents;
using WebSub.AspNetCore.Services;
using WebSub.Net.Http.Subscriber;
using Demo.AspNetCore.WebSub.Subscriber.Services;

namespace Demo.AspNetCore.WebSub.Subscriber
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient<WebSubSubscriber>();

            services.AddServerSentEvents();

            services.AddWebSubSubscriptionStore<MemoryWebSubSubscriptionsStore>();
            services.AddSingleton<IWebSubSubscriptionsService, ServerSentEventWebSubSubscriptionsService>();

            services.AddMvc()
                .AddWebSubWebHooks()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.MapServerSentEvents("/sse/webhooks/incoming/websub");

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller}/{action=Index}");
            });

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("-- Demo.AspNetCore.WebSub.Subscriber --");
            });
        }
    }
}
