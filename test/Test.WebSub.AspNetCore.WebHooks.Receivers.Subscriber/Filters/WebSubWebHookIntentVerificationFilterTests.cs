using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using WebSub.WebHooks.Receivers.Subscriber.Services;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters;
using Test.WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters.Infrastructure;

namespace Test.WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters
{
    public class WebSubWebHookIntentVerificationFilterTests
    {
        #region Fields
        private const string INTENT_VERIFICATION_MODE_DENIED = "denied";
        private const string INTENT_VERIFICATION_MODE_SUBSCRIBE = "subscribe";
        private const string INTENT_VERIFICATION_MODE_UNSUBSCRIBE = "unsubscribe";

        private const string WEBHOOK_ID = "73481a8e-c9ee-4ec4-89e3-b25b3179ae92";

        private const string WEBSUB_ROCKS_TOPIC_URL = "https://websub.rocks/blog/100/";
        private const string OTHER_WEBSUB_ROCKS_TOPIC_URL = "https://websub.rocks/blog/101/";
        private const string WEBSUB_ROCKS_CHALLENGE = "LNecT715EcOqAdVDWbVH";
        private const string WEBSUB_ROCKS_LEASE_SECONDS = "86400";
        #endregion

        #region Prepare SUT
        private WebSubSubscription PrepareWebSubSubscription(
            string subscriptionId = WEBHOOK_ID,
            string subscriptionTopicUrl = WEBSUB_ROCKS_TOPIC_URL,
            WebSubSubscriptionState subscriptionState = WebSubSubscriptionState.SubscribeRequested)
        {
            return new WebSubSubscription { Id = subscriptionId, TopicUrl = subscriptionTopicUrl, State = subscriptionState };
        }

        private WebSubRequestServices PrepareWebSubRequestServices(WebSubSubscription subscription = null)
        {
            subscription = subscription ?? PrepareWebSubSubscription();

            return new WebSubRequestServices(new Dictionary<string, WebSubSubscription>
            {
                { subscription.Id, subscription }
            });
        }

        private ResourceExecutingContext PrepareIntentVerificatioResourceExecutingContext(
            string mode = INTENT_VERIFICATION_MODE_SUBSCRIBE,
            string topic = WEBSUB_ROCKS_TOPIC_URL,
            string challenge = WEBSUB_ROCKS_CHALLENGE,
            string leaseSeconds = WEBSUB_ROCKS_LEASE_SECONDS,
            string reason = null,
            WebSubSubscription subscription = null,
            IServiceProvider requestServices = null)
        {
            subscription = subscription ?? PrepareWebSubSubscription();

            return new ResourceExecutingContext(new ActionContext()
            {
                ActionDescriptor = new ActionDescriptor(),
                HttpContext = new IntentVerificationHttpContext(mode, topic, challenge, leaseSeconds, reason, subscription, requestServices ?? PrepareWebSubRequestServices(subscription)),
                RouteData = new RouteData()
            }, new List<IFilterMetadata>(), new List<IValueProviderFactory>());
        }

        private WebSubWebHookIntentVerificationFilter PrepareWebSubWebHookIntentVerificationFilter()
        {
            return new WebSubWebHookIntentVerificationFilter(NullLoggerFactory.Instance);
        }
        #endregion

        #region Tests
        [Theory]
        [InlineData(INTENT_VERIFICATION_MODE_DENIED)]
        [InlineData(INTENT_VERIFICATION_MODE_SUBSCRIBE)]
        [InlineData(INTENT_VERIFICATION_MODE_UNSUBSCRIBE)]
        public async void OnResourceExecutionAsync_IntentVerificationRequest_DoesNotCallNext(string mode)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(mode: mode);
            Mock<ResourceExecutionDelegate> resourceExecutionDelegateMock = new Mock<ResourceExecutionDelegate>();

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegateMock.Object);

            resourceExecutionDelegateMock.Verify(m => m(), Times.Never);
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

        [Theory]
        [InlineData(INTENT_VERIFICATION_MODE_DENIED)]
        [InlineData(INTENT_VERIFICATION_MODE_SUBSCRIBE)]
        [InlineData(INTENT_VERIFICATION_MODE_UNSUBSCRIBE)]
        public async void OnResourceExecutionAsync_IntentVerificationRequestWithoutTopicParameter_SetsBadRequestObjectResult(string mode)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(mode: mode, topic: null);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<BadRequestObjectResult>(resourceExecutingContext.Result);
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

        [Theory]
        [InlineData(INTENT_VERIFICATION_MODE_SUBSCRIBE, WebSubSubscriptionState.SubscribeRequested)]
        [InlineData(INTENT_VERIFICATION_MODE_UNSUBSCRIBE, WebSubSubscriptionState.UnsubscribeRequested)]
        public async void OnResourceExecutionAsync_SubscribeUnsubscribeIntentVerificationRequest_SetsContentResult(string mode, WebSubSubscriptionState webSubSubscriptionState)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(mode: mode, subscription: PrepareWebSubSubscription(subscriptionState: webSubSubscriptionState));
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<ContentResult>(resourceExecutingContext.Result);
        }

        [Theory]
        [InlineData(INTENT_VERIFICATION_MODE_SUBSCRIBE, WebSubSubscriptionState.SubscribeRequested)]
        [InlineData(INTENT_VERIFICATION_MODE_UNSUBSCRIBE, WebSubSubscriptionState.UnsubscribeRequested)]
        public async void OnResourceExecutionAsync_SubscribeUnsubscribeIntentVerificationRequest_ContentResultContainsChallenge(string mode, WebSubSubscriptionState webSubSubscriptionState)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(mode: mode, subscription: PrepareWebSubSubscription(subscriptionState: webSubSubscriptionState));
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.Equal(WEBSUB_ROCKS_CHALLENGE, (resourceExecutingContext.Result as ContentResult).Content);
        }

        [Theory]
        [InlineData(INTENT_VERIFICATION_MODE_SUBSCRIBE, WebSubSubscriptionState.SubscribeRequested)]
        [InlineData(INTENT_VERIFICATION_MODE_UNSUBSCRIBE, WebSubSubscriptionState.UnsubscribeRequested)]
        public async void OnResourceExecutionAsync_SubscribeUnsubscribeVerificationRequestWithoutChallengeParameter_SetsBadRequestObjectResult(string mode, WebSubSubscriptionState webSubSubscriptionState)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(mode: mode, challenge: null, subscription: PrepareWebSubSubscription(subscriptionState: webSubSubscriptionState));
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<BadRequestObjectResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequestWithoutLeaseSecondsParameter_SetsBadRequestObjectResult()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(mode: INTENT_VERIFICATION_MODE_SUBSCRIBE, leaseSeconds: null, subscription: PrepareWebSubSubscription(subscriptionState: WebSubSubscriptionState.SubscribeRequested));
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<BadRequestObjectResult>(resourceExecutingContext.Result);
        }

        [Theory]
        [InlineData(WebSubSubscriptionState.Created)]
        [InlineData(WebSubSubscriptionState.SubscribeDenied)]
        [InlineData(WebSubSubscriptionState.SubscribeValidated)]
        [InlineData(WebSubSubscriptionState.UnsubscribeRequested)]
        [InlineData(WebSubSubscriptionState.UnsubscribeValidated)]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequestForSubscriptionStateDifferentThanSubscribeRequested_SetsNotFoundResult(WebSubSubscriptionState webSubSubscriptionState)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(mode: INTENT_VERIFICATION_MODE_SUBSCRIBE, subscription: PrepareWebSubSubscription(subscriptionState: webSubSubscriptionState));
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<NotFoundResult>(resourceExecutingContext.Result);
        }

        [Theory]
        [InlineData(WebSubSubscriptionState.Created)]
        [InlineData(WebSubSubscriptionState.SubscribeRequested)]
        [InlineData(WebSubSubscriptionState.SubscribeDenied)]
        [InlineData(WebSubSubscriptionState.SubscribeValidated)]
        [InlineData(WebSubSubscriptionState.UnsubscribeValidated)]
        public async void OnResourceExecutionAsync_UnsubscribeIntentVerificationRequestForSubscriptionStateDifferentThanUnsubscribeRequested_SetsNotFoundResult(WebSubSubscriptionState webSubSubscriptionState)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(mode: INTENT_VERIFICATION_MODE_UNSUBSCRIBE, subscription: PrepareWebSubSubscription(subscriptionState: webSubSubscriptionState));
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<NotFoundResult>(resourceExecutingContext.Result);
        }

        [Theory]
        [InlineData(INTENT_VERIFICATION_MODE_SUBSCRIBE, WebSubSubscriptionState.SubscribeRequested)]
        [InlineData(INTENT_VERIFICATION_MODE_UNSUBSCRIBE, WebSubSubscriptionState.UnsubscribeRequested)]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequestForNotMatchingTopic_SetsNotFoundResult(string mode, WebSubSubscriptionState webSubSubscriptionState)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareIntentVerificatioResourceExecutingContext(mode: mode, topic: OTHER_WEBSUB_ROCKS_TOPIC_URL, subscription: PrepareWebSubSubscription(subscriptionState: webSubSubscriptionState));
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<NotFoundResult>(resourceExecutingContext.Result);
        }
        #endregion
    }
}
