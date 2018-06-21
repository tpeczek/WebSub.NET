using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using WebSub.AspNetCore.Services;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters;
using Test.WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters.Infrastructure;
using Microsoft.AspNetCore.Hosting.Internal;

namespace Test.WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters
{
    public class WebSubWebHookSecurityFilterTests
    {
        #region Fields
        private const string HTTP_CONTEXT_ITEMS_SUBSCRIPTION_KEY = "WebSub.AspNetCore.WebHooks.Receivers.Subscriber-" + nameof(WebSubSubscription);

        private const string WEBHOOK_ID = "73481a8e-c9ee-4ec4-89e3-b25b3179ae92";
        private const string OTHER_WEBHOOK_ID = "24bacd35-cf3e-4ffe-811c-6278d339c11d";
        #endregion

        #region Prepare SUT
        private WebSubRequestServices PrepareWebSubRequestServices(
            string subscriptionId = WEBHOOK_ID,
            WebSubSubscriptionState subscriptionState = WebSubSubscriptionState.SubscribeValidated)
        {
            return new WebSubRequestServices(new Dictionary<string, WebSubSubscription>
            {
                { subscriptionId, new WebSubSubscription { Id = subscriptionId, State = subscriptionState } }
            });
        }

        private ResourceExecutingContext PrepareWebSubExecutingContext(
            string method,
            string id = WEBHOOK_ID,
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
                HttpContext = new WebSubHttpContext(method, requestServices ?? PrepareWebSubRequestServices()),
                RouteData = routeData
            }, new List<IFilterMetadata>(), new List<IValueProviderFactory>());
        }

        private WebSubWebHookSecurityFilter PrepareWebSubWebHookSecurityFilter()
        {
            return new WebSubWebHookSecurityFilter(
                new ConfigurationBuilder().Build(),
                new HostingEnvironment { EnvironmentName = "Production" },
                NullLoggerFactory.Instance);
        }
        #endregion

        #region Tests
        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        public async void OnResourceExecutionAsync_WebSubRequestWithoutId_SetsNotFoundResult(string httpMethod)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareWebSubExecutingContext(httpMethod, id: null);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookSecurityFilter webSubWebHookSecurityFilter = PrepareWebSubWebHookSecurityFilter();

            await webSubWebHookSecurityFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<NotFoundResult>(resourceExecutingContext.Result);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        public async void OnResourceExecutionAsync_WebSubRequestWithoutMatchingId_SetsNotFoundResult(string httpMethod)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareWebSubExecutingContext(httpMethod, id: OTHER_WEBHOOK_ID);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookSecurityFilter webSubWebHookSecurityFilter = PrepareWebSubWebHookSecurityFilter();

            await webSubWebHookSecurityFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<NotFoundResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async void OnResourceExecutionAsync_IntentVerificationRequest_SetsSubscriptionHttpContextItem()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareWebSubExecutingContext(HttpMethods.Get);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookSecurityFilter webSubWebHookSecurityFilter = PrepareWebSubWebHookSecurityFilter();

            await webSubWebHookSecurityFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<WebSubSubscription>(resourceExecutingContext.HttpContext.Items[HTTP_CONTEXT_ITEMS_SUBSCRIPTION_KEY]);
        }

        [Fact]
        public async void OnResourceExecutionAsync_IntentVerificationRequest_CallsNext()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareWebSubExecutingContext(HttpMethods.Get);
            Mock<ResourceExecutionDelegate> resourceExecutionDelegateMock = new Mock<ResourceExecutionDelegate>();

            WebSubWebHookSecurityFilter webSubWebHookSecurityFilter = PrepareWebSubWebHookSecurityFilter();

            await webSubWebHookSecurityFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegateMock.Object);

            resourceExecutionDelegateMock.Verify(m => m(), Times.Once);
        }

        [Theory]
        [InlineData(WebSubSubscriptionState.Created)]
        [InlineData(WebSubSubscriptionState.SubscribeDenied)]
        [InlineData(WebSubSubscriptionState.SubscribeRequested)]
        [InlineData(WebSubSubscriptionState.UnsubscribeRequested)]
        [InlineData(WebSubSubscriptionState.UnsubscribeValidated)]
        public async void OnResourceExecutionAsync_ContentDistributionRequestForSubscriptionStateDifferentThanSubscribeValidated_SetsNotFoundResult(WebSubSubscriptionState webSubSubscriptionState)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareWebSubExecutingContext(HttpMethods.Post, requestServices: PrepareWebSubRequestServices(subscriptionState: webSubSubscriptionState));
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookSecurityFilter webSubWebHookSecurityFilter = PrepareWebSubWebHookSecurityFilter();

            await webSubWebHookSecurityFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<NotFoundResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async void OnResourceExecutionAsync_ContentDistributionRequest_SetsSubscriptionHttpContextItem()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareWebSubExecutingContext(HttpMethods.Post);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookSecurityFilter webSubWebHookSecurityFilter = PrepareWebSubWebHookSecurityFilter();

            await webSubWebHookSecurityFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<WebSubSubscription>(resourceExecutingContext.HttpContext.Items[HTTP_CONTEXT_ITEMS_SUBSCRIPTION_KEY]);
        }

        [Fact]
        public async void OnResourceExecutionAsync_ContentDistributionRequest_CallsNext()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareWebSubExecutingContext(HttpMethods.Post);
            Mock<ResourceExecutionDelegate> resourceExecutionDelegateMock = new Mock<ResourceExecutionDelegate>();

            WebSubWebHookSecurityFilter webSubWebHookSecurityFilter = PrepareWebSubWebHookSecurityFilter();

            await webSubWebHookSecurityFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegateMock.Object);

            resourceExecutionDelegateMock.Verify(m => m(), Times.Once);
        }
        #endregion
    }
}
