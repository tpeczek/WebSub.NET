using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using WebSub.Net.Http.Subscriber;

namespace Demo.AspNetCore.WebSub
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient<WebSubSubscriber>();
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

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller}/{action=Index}");
            });

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("-- Demo.AspNetCore.WebSub --");
            });
        }
    }
}
