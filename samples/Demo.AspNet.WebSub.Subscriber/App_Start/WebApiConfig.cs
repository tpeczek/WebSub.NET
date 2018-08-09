using System.Web.Http;
using Unity;
using Unity.Lifetime;
using WebSub.WebHooks.Receivers.Subscriber.Services;
using Demo.AspNet.WebSub.Subscriber.Services;

namespace Demo.AspNet.WebSub.Subscriber
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API services
            UnityContainer container = new UnityContainer();
            container.RegisterType<IWebSubSubscriptionsStore, MemoryWebSubSubscriptionsStore>(new HierarchicalLifetimeManager());
            config.DependencyResolver = new UnityDependencyResolver(container);

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
