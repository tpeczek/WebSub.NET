using Microsoft.AspNetCore.Http;

namespace WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="HttpRequest" /> to support WebSub WebHooks.
    /// </summary>
    public static class WebSubHttpRequestExtensions
    {
        /// <summary>
        /// Gets WebSub WebHook URL.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <returns></returns>
        public static string GetWebSubWebHookUrl(this HttpRequest request, string subscriptionId)
        {
            PathString hostPathString = new PathString("//" + request.Host);
            PathString webHookPathString = new PathString("/api/webhooks/incoming/websub/");

            return request.Scheme + ":" + hostPathString.Add(request.PathBase).Add(webHookPathString).Value + subscriptionId;
        }
    }
}
