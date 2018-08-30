using System.Net.Http;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using WebSub.WebHooks.Receivers.Subscriber.Services;

namespace WebSub.AspNet.WebHooks.Receivers.Subscriber.Extensions
{
    internal static class HttpRequestMessageExtensions
    {
        public static IWebSubSubscriptionsStore GetWebSubSubscriptionsStore(this HttpRequestMessage request)
        {
            return request.GetConfiguration().DependencyResolver.GetService<IWebSubSubscriptionsStore>();
        }

        public static IWebSubSubscriptionsService GetWebSubSubscriptionsService(this HttpRequestMessage request)
        {
            return request.GetConfiguration().DependencyResolver.GetService<IWebSubSubscriptionsService>();
        }

        public static ILogger GetWebHooksLogger(this HttpRequestMessage request)
        {
            return request.GetConfiguration().DependencyResolver.GetLogger();
        }
    }
}
