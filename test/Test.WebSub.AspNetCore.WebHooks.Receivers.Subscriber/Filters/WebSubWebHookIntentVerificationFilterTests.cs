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

        private ResourceExecutingContext PrepareSubscribeIntentVerificationResourceExecutingContext(
            string id = WEBHOOK_ID,
            string mode = INTENT_VERIFICATION_MODE_SUBSCRIBE,
            string topic = WEBSUB_ROCKS_TOPIC_URL,
            string challenge = WEBSUB_ROCKS_CHALLENGE,
            string leaseSeconds = WEBSUB_ROCKS_LEASE_SECONDS,
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
                HttpContext = new IntentVerificationHttpContext(mode, topic, challenge, leaseSeconds, requestServices ?? PrepareWebSubRequestServices()),
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
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequestWithoutId_CallsNext()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareSubscribeIntentVerificationResourceExecutingContext(id: null);
            Mock<ResourceExecutionDelegate> resourceExecutionDelegateMock = new Mock<ResourceExecutionDelegate>();

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegateMock.Object);

            resourceExecutionDelegateMock.Verify(m => m(), Times.Once);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequestWithoutModeParameter_CallsNext()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareSubscribeIntentVerificationResourceExecutingContext(mode: null);
            Mock<ResourceExecutionDelegate> resourceExecutionDelegateMock = new Mock<ResourceExecutionDelegate>();

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegateMock.Object);

            resourceExecutionDelegateMock.Verify(m => m(), Times.Once);
        }

        [Fact]
        public async void OnResourceExecutionAsync_CorrectSubscribeIntentVerificationRequest_DoesNotCallNext()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareSubscribeIntentVerificationResourceExecutingContext();
            Mock<ResourceExecutionDelegate> resourceExecutionDelegateMock = new Mock<ResourceExecutionDelegate>();

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegateMock.Object);

            resourceExecutionDelegateMock.Verify(m => m(), Times.Never);
        }

        [Fact]
        public async void OnResourceExecutionAsync_CorrectSubscribeIntentVerificationRequest_SetsContentResult()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareSubscribeIntentVerificationResourceExecutingContext();
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<ContentResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async void OnResourceExecutionAsync_CorrectSubscribeIntentVerificationRequest_ContentResultContainsChallenge()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareSubscribeIntentVerificationResourceExecutingContext();
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.Equal(WEBSUB_ROCKS_CHALLENGE, (resourceExecutingContext.Result as ContentResult).Content);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequestWithoutTopicParameter_SetsBadRequestObjectResult()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareSubscribeIntentVerificationResourceExecutingContext(topic: null);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<BadRequestObjectResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequestWithoutChallengeParameter_SetsBadRequestObjectResult()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareSubscribeIntentVerificationResourceExecutingContext(challenge: null);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<BadRequestObjectResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequestWithoutLeaseSecondsParameter_SetsBadRequestObjectResult()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareSubscribeIntentVerificationResourceExecutingContext(leaseSeconds: null);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<BadRequestObjectResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequestWithoutMatchingId_SetsNotFoundResult()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareSubscribeIntentVerificationResourceExecutingContext(id: OTHER_WEBHOOK_ID);
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
            ResourceExecutingContext resourceExecutingContext = PrepareSubscribeIntentVerificationResourceExecutingContext(requestServices: PrepareWebSubRequestServices(subscriptionState: webSubSubscriptionState));
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<NotFoundResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async void OnResourceExecutionAsync_SubscribeIntentVerificationRequestForNotMatchingTopic_SetsNotFoundResult()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareSubscribeIntentVerificationResourceExecutingContext(topic: OTHER_WEBSUB_ROCKS_TOPIC_URL);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookIntentVerificationFilter webSubWebHookIntentVerificationFilter = PrepareWebSubWebHookIntentVerificationFilter();

            await webSubWebHookIntentVerificationFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<NotFoundResult>(resourceExecutingContext.Result);
        }
        #endregion
    }
}
