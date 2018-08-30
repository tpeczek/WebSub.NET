using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Text;
using System.Threading.Tasks;
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
        private const string INTENT_VERIFICATION_MODE_QUERY_PARAMETER_NAME = "hub.mode";
        private const string INTENT_VERIFICATION_TOPIC_QUERY_PARAMETER_NAME = "hub.topic";
        private const string INTENT_VERIFICATION_CHALLENGE_QUERY_PARAMETER_NAME = "hub.challenge";
        private const string INTENT_VERIFICATION_LEASE_SECONDS_QUERY_PARAMETER_NAME = "hub.lease_seconds";
        private const string INTENT_VERIFICATION_REASON_QUERY_PARAMETER_NAME = "hub.reason";

        private const string INTENT_VERIFICATION_MODE_DENIED = "denied";
        private const string INTENT_VERIFICATION_MODE_SUBSCRIBE = "subscribe";
        private const string INTENT_VERIFICATION_MODE_UNSUBSCRIBE = "unsubscribe";

        private const string WEBHOOK_BASE_URI = "https://localhost/api/webhooks/incoming/websub/";

        private const string WEBHOOK_ID = "73481a8e-c9ee-4ec4-89e3-b25b3179ae92";
        private const string OTHER_WEBHOOK_ID = "24bacd35-cf3e-4ffe-811c-6278d339c11d";

        private const string WEBSUB_ROCKS_TOPIC_URL = "https://websub.rocks/blog/100/";
        private const string OTHER_WEBSUB_ROCKS_TOPIC_URL = "https://websub.rocks/blog/101/";
        private const string WEBSUB_ROCKS_CHALLENGE = "LNecT715EcOqAdVDWbVH";
        private const string WEBSUB_ROCKS_LEASE_SECONDS = "86400";
        #endregion

        #region Prepare SUT
        private WebSubDependencyResolver PrepareWebSubDependencyResolver(
            string subscriptionId,
            string subscriptionTopicUrl = WEBSUB_ROCKS_TOPIC_URL,
            WebSubSubscriptionState subscriptionState = WebSubSubscriptionState.SubscribeValidated,
            string secret = null)
        {
            return new WebSubDependencyResolver(new Dictionary<string, WebSubSubscription>
            {
                { subscriptionId, new WebSubSubscription { Id = subscriptionId, State = subscriptionState, TopicUrl = subscriptionTopicUrl, Secret = secret } }
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

        private HttpRequestMessage PrepareIntentVerificatioRequestMessage(string id, HttpRequestContext context,
            string mode = INTENT_VERIFICATION_MODE_SUBSCRIBE,
            string topic = WEBSUB_ROCKS_TOPIC_URL,
            string challenge = WEBSUB_ROCKS_CHALLENGE,
            string leaseSeconds = WEBSUB_ROCKS_LEASE_SECONDS,
            string reason = null)
        {
            StringBuilder queryBuilder = new StringBuilder();

            if (!String.IsNullOrWhiteSpace(mode))
            {
                queryBuilder.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(INTENT_VERIFICATION_MODE_QUERY_PARAMETER_NAME), HttpUtility.UrlEncode(mode));
            }

            if (!String.IsNullOrWhiteSpace(topic))
            {
                queryBuilder.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(INTENT_VERIFICATION_TOPIC_QUERY_PARAMETER_NAME), HttpUtility.UrlEncode(topic));
            }

            if (!String.IsNullOrWhiteSpace(challenge))
            {
                queryBuilder.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(INTENT_VERIFICATION_CHALLENGE_QUERY_PARAMETER_NAME), HttpUtility.UrlEncode(challenge));
            }

            if (!String.IsNullOrWhiteSpace(leaseSeconds))
            {
                queryBuilder.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(INTENT_VERIFICATION_LEASE_SECONDS_QUERY_PARAMETER_NAME), HttpUtility.UrlEncode(leaseSeconds));
            }

            if (!String.IsNullOrWhiteSpace(reason))
            {
                queryBuilder.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(INTENT_VERIFICATION_REASON_QUERY_PARAMETER_NAME), HttpUtility.UrlEncode(reason));
            }

            string requestUri = WEBHOOK_BASE_URI + id;
            if (queryBuilder.Length > 0)
            {
                queryBuilder.Length--;
                requestUri = requestUri + "?" + queryBuilder.ToString();
            }

            HttpRequestMessage intentVerificatioRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);

            intentVerificatioRequestMessage.SetRequestContext(context);

            return intentVerificatioRequestMessage;
        }
        #endregion

        #region Tests
        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        public async Task OnReceiveAsync_WebSubRequestWithoutMatchingId_ReturnsNotFoundResponse(string httpMethod)
        {
            HttpRequestContext context = PrepareWebSubRequestContext(PrepareWebSubDependencyResolver(WEBHOOK_ID));
            HttpRequestMessage request = PrepareWebSubRequestMessage(httpMethod, WEBHOOK_ID, context);
            WebSubWebHookReceiver webSubWebHookReceiver = new WebSubWebHookReceiver();

            HttpResponseMessage receiveAsyncResult = await webSubWebHookReceiver.ReceiveAsync(OTHER_WEBHOOK_ID, context, request);

            Assert.Equal(HttpStatusCode.NotFound, receiveAsyncResult.StatusCode);
        }

        [Fact]
        public async Task OnReceiveAsync_IntentVerificationRequestWithoutModeParameter_ReturnsBadRequestResponse()
        {
            HttpRequestContext context = PrepareWebSubRequestContext(PrepareWebSubDependencyResolver(WEBHOOK_ID));
            HttpRequestMessage request = PrepareIntentVerificatioRequestMessage(WEBHOOK_ID, context, mode: null);
            WebSubWebHookReceiver webSubWebHookReceiver = new WebSubWebHookReceiver();

            HttpResponseMessage receiveAsyncResult = await webSubWebHookReceiver.ReceiveAsync(WEBHOOK_ID, context, request);

            Assert.Equal(HttpStatusCode.BadRequest, receiveAsyncResult.StatusCode);
        }

        [Theory]
        [InlineData(INTENT_VERIFICATION_MODE_DENIED)]
        [InlineData(INTENT_VERIFICATION_MODE_SUBSCRIBE)]
        [InlineData(INTENT_VERIFICATION_MODE_UNSUBSCRIBE)]
        public async Task OnReceiveAsync_IntentVerificationRequestWithoutTopicParameter_ReturnsBadRequestResponse(string mode)
        {
            HttpRequestContext context = PrepareWebSubRequestContext(PrepareWebSubDependencyResolver(WEBHOOK_ID));
            HttpRequestMessage request = PrepareIntentVerificatioRequestMessage(WEBHOOK_ID, context, mode: mode, topic: null);
            WebSubWebHookReceiver webSubWebHookReceiver = new WebSubWebHookReceiver();

            HttpResponseMessage receiveAsyncResult = await webSubWebHookReceiver.ReceiveAsync(WEBHOOK_ID, context, request);

            Assert.Equal(HttpStatusCode.BadRequest, receiveAsyncResult.StatusCode);
        }

        [Fact]
        public async Task OnReceiveAsync_SubscribeIntentDenyRequest_ReturnsNoContentResponse()
        {
            HttpRequestContext context = PrepareWebSubRequestContext(PrepareWebSubDependencyResolver(WEBHOOK_ID));
            HttpRequestMessage request = PrepareIntentVerificatioRequestMessage(WEBHOOK_ID, context, mode: INTENT_VERIFICATION_MODE_DENIED, challenge: null, leaseSeconds: null);
            WebSubWebHookReceiver webSubWebHookReceiver = new WebSubWebHookReceiver();

            HttpResponseMessage receiveAsyncResult = await webSubWebHookReceiver.ReceiveAsync(WEBHOOK_ID, context, request);

            Assert.Equal(HttpStatusCode.NoContent, receiveAsyncResult.StatusCode);
        }

        [Theory]
        [InlineData(INTENT_VERIFICATION_MODE_SUBSCRIBE, WebSubSubscriptionState.SubscribeRequested)]
        [InlineData(INTENT_VERIFICATION_MODE_UNSUBSCRIBE, WebSubSubscriptionState.UnsubscribeRequested)]
        public async Task OnReceiveAsync_SubscribeUnsubscribeIntentVerificationRequest_ReturnsOkResponse(string mode, WebSubSubscriptionState webSubSubscriptionState)
        {
            HttpRequestContext context = PrepareWebSubRequestContext(PrepareWebSubDependencyResolver(WEBHOOK_ID, subscriptionState: webSubSubscriptionState));
            HttpRequestMessage request = PrepareIntentVerificatioRequestMessage(WEBHOOK_ID, context, mode: mode);
            WebSubWebHookReceiver webSubWebHookReceiver = new WebSubWebHookReceiver();

            HttpResponseMessage receiveAsyncResult = await webSubWebHookReceiver.ReceiveAsync(WEBHOOK_ID, context, request);

            Assert.Equal(HttpStatusCode.OK, receiveAsyncResult.StatusCode);
        }

        [Theory]
        [InlineData(INTENT_VERIFICATION_MODE_SUBSCRIBE, WebSubSubscriptionState.SubscribeRequested)]
        [InlineData(INTENT_VERIFICATION_MODE_UNSUBSCRIBE, WebSubSubscriptionState.UnsubscribeRequested)]
        public async Task OnReceiveAsync_SubscribeUnsubscribeIntentVerificationRequest_ResponseContainsChallenge(string mode, WebSubSubscriptionState webSubSubscriptionState)
        {
            HttpRequestContext context = PrepareWebSubRequestContext(PrepareWebSubDependencyResolver(WEBHOOK_ID, subscriptionState: webSubSubscriptionState));
            HttpRequestMessage request = PrepareIntentVerificatioRequestMessage(WEBHOOK_ID, context, mode: mode);
            WebSubWebHookReceiver webSubWebHookReceiver = new WebSubWebHookReceiver();

            HttpResponseMessage receiveAsyncResult = await webSubWebHookReceiver.ReceiveAsync(WEBHOOK_ID, context, request);

            Assert.Equal(WEBSUB_ROCKS_CHALLENGE, await receiveAsyncResult.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData(INTENT_VERIFICATION_MODE_SUBSCRIBE, WebSubSubscriptionState.SubscribeRequested)]
        [InlineData(INTENT_VERIFICATION_MODE_UNSUBSCRIBE, WebSubSubscriptionState.UnsubscribeRequested)]
        public async Task OnReceiveAsync_SubscribeUnsubscribeVerificationRequestWithoutChallengeParameter_ReturnsBadRequestResponse(string mode, WebSubSubscriptionState webSubSubscriptionState)
        {
            HttpRequestContext context = PrepareWebSubRequestContext(PrepareWebSubDependencyResolver(WEBHOOK_ID, subscriptionState: webSubSubscriptionState));
            HttpRequestMessage request = PrepareIntentVerificatioRequestMessage(WEBHOOK_ID, context, mode: mode, challenge: null);
            WebSubWebHookReceiver webSubWebHookReceiver = new WebSubWebHookReceiver();

            HttpResponseMessage receiveAsyncResult = await webSubWebHookReceiver.ReceiveAsync(WEBHOOK_ID, context, request);

            Assert.Equal(HttpStatusCode.BadRequest, receiveAsyncResult.StatusCode);
        }

        [Fact]
        public async Task OnReceiveAsync_SubscribeIntentVerificationRequestWithoutLeaseSecondsParameter_ReturnsBadRequestResponse()
        {
            HttpRequestContext context = PrepareWebSubRequestContext(PrepareWebSubDependencyResolver(WEBHOOK_ID, subscriptionState: WebSubSubscriptionState.SubscribeRequested));
            HttpRequestMessage request = PrepareIntentVerificatioRequestMessage(WEBHOOK_ID, context, mode: INTENT_VERIFICATION_MODE_SUBSCRIBE, leaseSeconds: null);
            WebSubWebHookReceiver webSubWebHookReceiver = new WebSubWebHookReceiver();

            HttpResponseMessage receiveAsyncResult = await webSubWebHookReceiver.ReceiveAsync(WEBHOOK_ID, context, request);

            Assert.Equal(HttpStatusCode.BadRequest, receiveAsyncResult.StatusCode);
        }

        [Theory]
        [InlineData(WebSubSubscriptionState.Created)]
        [InlineData(WebSubSubscriptionState.SubscribeDenied)]
        [InlineData(WebSubSubscriptionState.SubscribeValidated)]
        [InlineData(WebSubSubscriptionState.UnsubscribeRequested)]
        [InlineData(WebSubSubscriptionState.UnsubscribeValidated)]
        public async Task OnReceiveAsync_SubscribeIntentVerificationRequestForSubscriptionStateDifferentThanSubscribeRequested_ReturnsNotFoundResponse(WebSubSubscriptionState webSubSubscriptionState)
        {
            HttpRequestContext context = PrepareWebSubRequestContext(PrepareWebSubDependencyResolver(WEBHOOK_ID, subscriptionState: webSubSubscriptionState));
            HttpRequestMessage request = PrepareIntentVerificatioRequestMessage(WEBHOOK_ID, context, mode: INTENT_VERIFICATION_MODE_SUBSCRIBE);
            WebSubWebHookReceiver webSubWebHookReceiver = new WebSubWebHookReceiver();

            HttpResponseMessage receiveAsyncResult = await webSubWebHookReceiver.ReceiveAsync(WEBHOOK_ID, context, request);

            Assert.Equal(HttpStatusCode.NotFound, receiveAsyncResult.StatusCode);
        }

        [Theory]
        [InlineData(WebSubSubscriptionState.Created)]
        [InlineData(WebSubSubscriptionState.SubscribeRequested)]
        [InlineData(WebSubSubscriptionState.SubscribeDenied)]
        [InlineData(WebSubSubscriptionState.SubscribeValidated)]
        [InlineData(WebSubSubscriptionState.UnsubscribeValidated)]
        public async Task OnReceiveAsync_UnsubscribeIntentVerificationRequestForSubscriptionStateDifferentThanUnsubscribeRequested_ReturnsNotFoundResponse(WebSubSubscriptionState webSubSubscriptionState)
        {
            HttpRequestContext context = PrepareWebSubRequestContext(PrepareWebSubDependencyResolver(WEBHOOK_ID, subscriptionState: webSubSubscriptionState));
            HttpRequestMessage request = PrepareIntentVerificatioRequestMessage(WEBHOOK_ID, context, mode: INTENT_VERIFICATION_MODE_UNSUBSCRIBE);
            WebSubWebHookReceiver webSubWebHookReceiver = new WebSubWebHookReceiver();

            HttpResponseMessage receiveAsyncResult = await webSubWebHookReceiver.ReceiveAsync(WEBHOOK_ID, context, request);

            Assert.Equal(HttpStatusCode.NotFound, receiveAsyncResult.StatusCode);
        }

        [Theory]
        [InlineData(INTENT_VERIFICATION_MODE_SUBSCRIBE, WebSubSubscriptionState.SubscribeRequested)]
        [InlineData(INTENT_VERIFICATION_MODE_UNSUBSCRIBE, WebSubSubscriptionState.UnsubscribeRequested)]
        public async Task OnReceiveAsync_SubscribeIntentVerificationRequestForNotMatchingTopic_ReturnsNotFoundResponse(string mode, WebSubSubscriptionState webSubSubscriptionState)
        {
            HttpRequestContext context = PrepareWebSubRequestContext(PrepareWebSubDependencyResolver(WEBHOOK_ID, subscriptionState: webSubSubscriptionState));
            HttpRequestMessage request = PrepareIntentVerificatioRequestMessage(WEBHOOK_ID, context, mode: mode, topic: OTHER_WEBSUB_ROCKS_TOPIC_URL);
            WebSubWebHookReceiver webSubWebHookReceiver = new WebSubWebHookReceiver();

            HttpResponseMessage receiveAsyncResult = await webSubWebHookReceiver.ReceiveAsync(WEBHOOK_ID, context, request);

            Assert.Equal(HttpStatusCode.NotFound, receiveAsyncResult.StatusCode);
        }
        #endregion
    }
}
