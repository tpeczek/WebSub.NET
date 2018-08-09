using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Microsoft.AspNet.WebHooks;
using WebSub.WebHooks.Receivers.Subscriber.Services;

namespace WebSub.AspNet.WebHooks.Receivers.Subscriber.WebHooks
{
    /// <summary>
    /// Provides an <see cref="IWebHookReceiver"/> implementation which supports WebSub subscriber WebHooks.
    /// </summary>
    public class WebSubWebHookReceiver : WebHookReceiver
    {
        #region Fields
        internal const string RECEIVER_NAME = "websub";
        #endregion

        #region Properties
        /// <summary>
        /// Gets the receiver name for this receiver.
        /// </summary>
        public static string ReceiverName => RECEIVER_NAME;

        /// <summary>
        /// Gets the case-insensitive name of the WebHook generator that this receiver supports, for example <c>dropbox</c> or <c>net</c>.
        ///  The name provided here will map to a URI of the form '<c>https://&lt;host&gt;/api/webhooks/incoming/&lt;name&gt;</c>'.
        /// </summary>
        public override string Name => RECEIVER_NAME;
        #endregion

        #region Methods
        /// <summary>
        /// Processes the incoming WebHook request. The request may be an initialization request or it may be 
        /// an actual WebHook request. It is up to the receiver to determine what kind of incoming request it
        /// is and process it accordingly.
        /// </summary>
        /// <param name="id">A (potentially empty) ID of a particular configuration for this <see cref="IWebHookReceiver"/>. This
        /// allows an <see cref="IWebHookReceiver"/> to support multiple WebHooks with individual configurations.</param>
        /// <param name="context">The <see cref="HttpRequestContext"/> for the incoming request.</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> containing the incoming WebHook.</param>
        public override async Task<HttpResponseMessage> ReceiveAsync(string id, HttpRequestContext context, HttpRequestMessage request)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            WebSubSubscription subscription = await RetrieveWebSubSubscriptionAsync(request, id);
            if (subscription != null)
            {
                if (request.Method == HttpMethod.Get)
                {
                    return await HandleIntentVerificationAsync(subscription, request);
                }
                else if (request.Method == HttpMethod.Post)
                {
                    return await ExecuteWebHookAsync(id, context, request, Enumerable.Empty<string>(), null);
                }
                else
                {
                    return CreateBadMethodResponse(request);
                }
            }
            else
            {
                return request.CreateErrorResponse(HttpStatusCode.NotFound, String.Empty);
            }
        }

        private static Task<WebSubSubscription> RetrieveWebSubSubscriptionAsync(HttpRequestMessage request, string subscriptionId)
        {
            IWebSubSubscriptionsStore subscriptionsStore = request.GetConfiguration().DependencyResolver.GetService<IWebSubSubscriptionsStore>();

            return subscriptionsStore.RetrieveAsync(subscriptionId);
        }

        private static Task<HttpResponseMessage> HandleIntentVerificationAsync(WebSubSubscription subscription, HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
