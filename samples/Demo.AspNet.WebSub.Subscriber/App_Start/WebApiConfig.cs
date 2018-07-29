using System.Web.Http;

namespace Demo.AspNet.WebSub.Subscriber
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Initialize WebSub WebHook receiver
            config.InitializeReceiveWebSubWebHooks();
        }
    }
}
