using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lib.AspNetCore.ServerSentEvents;
using WebSub.Net.Http.Subscriber;
using WebSub.WebHooks.Receivers.Subscriber.Services;
using Demo.AspNetCore.WebSub.Subscriber.Model;
using Demo.AspNetCore.WebSub.Subscriber.Services;

namespace Demo.AspNetCore.WebSub.Subscriber
{
    public class Startup
    {
        private const string SQLITE_CONNECTION_STRING_NAME = "ApplicationSqliteDatabase";

        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient<WebSubSubscriber>();

            services.AddServerSentEvents();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString(SQLITE_CONNECTION_STRING_NAME))
            );
            services.AddEntityFrameworkWebSubSubscriptionStore<ApplicationDbContext>();

            //services.AddSingleton<IWebSubSubscriptionsStore, MemoryWebSubSubscriptionsStore>();

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

            EnsureDatabaseCreated(app);

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

        private static IApplicationBuilder EnsureDatabaseCreated(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                ApplicationDbContext context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                if (context != null)
                {
                    context.Database.EnsureCreated();
                }
            }

            return app;
        }
    }
}
