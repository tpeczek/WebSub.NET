using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Collections.Generic;
using Xunit;
using WebSub.WebHooks.Receivers.Subscriber.Services;
using WebSub.AspNet.WebHooks.Receivers.Subscriber.WebHooks;
using Test.WebSub.AspNet.WebHooks.Receivers.Subscriber.WebHooks.Infrastructure;

namespace Test.WebSub.AspNet.WebHooks.Receivers.Subscriber.WebHooks
{
    public class WebSubWebHookReceiverTests
    {
        #region Fields
        private const string WEBHOOK_BASE_URI = "/api/webhooks/incoming/websub/";

        private const string WEBHOOK_ID = "73481a8e-c9ee-4ec4-89e3-b25b3179ae92";
        private const string OTHER_WEBHOOK_ID = "24bacd35-cf3e-4ffe-811c-6278d339c11d";
        #endregion

        #region Prepare SUT
        private WebSubDependencyResolver PrepareWebSubDependencyResolver(
            string subscriptionId,
            WebSubSubscriptionState subscriptionState = WebSubSubscriptionState.SubscribeValidated,
            string secret = null)
        {
            return new WebSubDependencyResolver(new Dictionary<string, WebSubSubscription>
            {
                { subscriptionId, new WebSubSubscription { Id = subscriptionId, State = subscriptionState, Secret = secret } }
            });
        }

        private HttpRequestContext PrepareWebSubRequestContext(WebSubDependencyResolver dependencyResolver)
        {
            return new HttpRequestContext
            {
                Configuration = new HttpConfiguration
                {
                    DependencyResolver = dependencyResolver
                }
            };
        }

        private HttpRequestMessage PrepareWebSubRequestMessage(string method, string id, HttpRequestContext context)
        {
            HttpRequestMessage webSubRequestMessage = new HttpRequestMessage(new HttpMethod(method), WEBHOOK_BASE_URI + id);

            webSubRequestMessage.SetRequestContext(context);

            return webSubRequestMessage;
        }
        #endregion

        #region Tests
        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        public async void OnReceiveAsync_WebSubRequestWithoutMatchingId_ReturnsNotFoundResponse(string httpMethod)
        {
            HttpRequestContext context = PrepareWebSubRequestContext(PrepareWebSubDependencyResolver(WEBHOOK_ID));
            HttpRequestMessage request = PrepareWebSubRequestMessage(httpMethod, WEBHOOK_ID, context);
            WebSubWebHookReceiver webSubWebHookReceiver = new WebSubWebHookReceiver();

            HttpResponseMessage receiveAsyncResult = await webSubWebHookReceiver.ReceiveAsync(OTHER_WEBHOOK_ID, context, request);

            Assert.Equal(HttpStatusCode.NotFound, receiveAsyncResult.StatusCode);
        }
        #endregion
    }
}
