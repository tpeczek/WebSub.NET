using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters;
using Test.WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters.Infrastructure;
using WebSub.AspNetCore.Services;
using System.Threading.Tasks;

namespace Test.WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters
{
    public class WebSubWebHookIntentVerificationFilterTests
    {
        #region Fields
        private const string INTENT_VERIFICATION_MODE_DENIED = "denied";
        private const string INTENT_VERIFICATION_MODE_SUBSCRIBE = "subscribe";
        private const string INTENT_VERIFICATION_MODE_UNSUBSCRIBE = "unsubscribe";

        internal const string WEBHOOK_ID = "73481a8e-c9ee-4ec4-89e3-b25b3179ae92";
        internal const string OTHER_WEBHOOK_ID = "24bacd35-cf3e-4ffe-811c-6278d339c11d";

        internal const string WEBSUB_ROCKS_TOPIC_URL = "https://websub.rocks/blog/100/";
        internal const string OTHER_WEBSUB_ROCKS_TOPIC_URL = "https://websub.rocks/blog/101/";
        internal const string WEBSUB_ROCKS_CHALLENGE = "LNecT715EcOqAdVDWbVH";
        internal const string WEBSUB_ROCKS_LEASE_SECONDS = "86400";
        #endregion

        #region Prepare SUT
        private WebSubRequestServices PrepareWebSubRequestServices(
            string subscriptionId = WEBHOOK_ID,
            string subscriptionTopicUrl = WEBSUB_ROCKS_TOPIC_URL,
            WebSubSubscriptionState subscriptionState = WebSubSubscriptionState.SubscribeRequested)
        {
            return new WebSubRequestServices(new Dictionary<string, WebSubSubscription>
            {
                {subscriptionId, new WebSubSubscription { Id = subscriptionId, TopicUrl = subscriptionTopicUrl, State = subscriptionState } }
            });
        }

        private ResourceExecutingContext PrepareIntentVerificatioResourceExecutingContext(
            string id = WEBHOOK_ID,
            string mode = INTENT_VERIFICATION_MODE_SUBSCRIBE,
            string topic = WEBSUB_ROCKS_TOPIC_URL,
            string challenge = WEBSUB_ROCKS_CHALLENGE,
            string leaseSeconds = WEBSUB_ROCKS_LEASE_SECONDS,
            string reason = null,
            IServiceProvider requestServices = null)
        {
            RouteData routeData = new RouteData();

            if (!String.IsNullOrWhiteSpace(id))
            {
                routeData.Values.Add("id", id);
            }

            return new ResourceExecutingContext(new ActionContext()
            {
                ActionDescriptor = new ActionDescriptor(),
                HttpContext = new IntentVerificationHttpContext(mode, topic, challenge, leaseSeconds, reason, requestServices ?? PrepareWebSubRequestServices()),
                RouteData = routeData
            }, new List<IFilterMetadata>(), new List<IValueProviderFactory>());
        }

        private WebSubWebHookIntentVerificationFilter PrepareWebSubWebHookIntentVerificationFilter()
        {
            return new WebSubWebHookIntentVerificationFilter(NullLoggerFactory.Instance);
        }
        #endregion

        #region Tests
        [Fact]
        public async void OnResourceExecutionAsync_IntentVerificationRequestWithoutId_CallsNext()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(id: null);
            Mock<ResourceExecutionDelegate> resourceExecutionDelegateMock = new Mock<ResourceExecutionDelegate>();

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegateMock.Object);

            resourceExecutionDelegateMock.Verify(m => m(), Times.Once);
        }

        [Fact]
        public async void OnResourceExecutionAsync_IntentVerificationRequestWithoutModeParameter_CallsNext()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(mode: null);
            Mock<ResourceExecutionDelegate> resourceExecutionDelegateMock = new Mock<ResourceExecutionDelegate>();

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegateMock.Object);

            resourceExecutionDelegateMock.Verify(m => m(), Times.Once);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentDenyRequest_DoesNotCallNext()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(mode: INTENT_VERIFICATION_MODE_DENIED, challenge: null, leaseSeconds: null);
            Mock<ResourceExecutionDelegate> resourceExecutionDelegateMock = new Mock<ResourceExecutionDelegate>();

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegateMock.Object);

            resourceExecutionDelegateMock.Verify(m => m(), Times.Never);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentDenyRequest_SetsNoContentResult()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(mode: INTENT_VERIFICATION_MODE_DENIED, challenge: null, leaseSeconds: null);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<NoContentResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentDenyRequestWithoutTopicParameter_SetsBadRequestObjectResult()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(mode: INTENT_VERIFICATION_MODE_DENIED, topic: null, challenge: null, leaseSeconds: null);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<BadRequestObjectResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequest_DoesNotCallNext()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext();
            Mock<ResourceExecutionDelegate> resourceExecutionDelegateMock = new Mock<ResourceExecutionDelegate>();

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegateMock.Object);

            resourceExecutionDelegateMock.Verify(m => m(), Times.Never);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequest_SetsContentResult()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext();
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<ContentResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequest_ContentResultContainsChallenge()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext();
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.Equal(WEBSUB_ROCKS_CHALLENGE, (resourceExecutingContext.Result as ContentResult).Content);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequestWithoutTopicParameter_SetsBadRequestObjectResult()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(topic: null);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<BadRequestObjectResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequestWithoutChallengeParameter_SetsBadRequestObjectResult()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(challenge: null);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<BadRequestObjectResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequestWithoutLeaseSecondsParameter_SetsBadRequestObjectResult()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(leaseSeconds: null);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<BadRequestObjectResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequestWithoutMatchingId_SetsNotFoundResult()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(id: OTHER_WEBHOOK_ID);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<NotFoundResult>(resourceExecutingContext.Result);
        }

        [Theory]
        [InlineData(WebSubSubscriptionState.Created)]
        [InlineData(WebSubSubscriptionState.SubscribeDenied)]
        [InlineData(WebSubSubscriptionState.SubscribeValidated)]
        [InlineData(WebSubSubscriptionState.UnsubscribeRequested)]
        [InlineData(WebSubSubscriptionState.UnsubscribeValidated)]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequestForSubscriptionStateDifferentThanRequested_SetsNotFoundResult(WebSubSubscriptionState webSubSubscriptionState)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(requestServices: PrepareWebSubRequestServices(subscriptionState: webSubSubscriptionState));
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<NotFoundResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequestForNotMatchingTopic_SetsNotFoundResult()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(topic: OTHER_WEBSUB_ROCKS_TOPIC_URL);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<NotFoundResult>(resourceExecutingContext.Result);
        }
        #endregion
    }
}
