using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using WebSub.WebHooks.Receivers.Subscriber.Services;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters;
using Test.WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters.Infrastructure;

namespace Test.WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters
{
    public class WebSubWebHookSecurityFilterTests
    {
        #region Fields
        private const string HTTP_CONTEXT_ITEMS_SUBSCRIPTION_KEY = "WebSub.AspNetCore.WebHooks.Receivers.Subscriber-" + nameof(WebSubSubscription);

        private const string WEBHOOK_ID = "73481a8e-c9ee-4ec4-89e3-b25b3179ae92";
        private const string OTHER_WEBHOOK_ID = "24bacd35-cf3e-4ffe-811c-6278d339c11d";

        private const string SECRET = "0123456789012345";
        private const string CONTENT = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
        private const string CONTENT_VALID_HMACSHA1 = "751fe4094ee7179246b218c044ef4cad22f5f15e";
        private const string CONTENT_INVALID_HMACSHA1 = "751fe4094ee7179246b218c044ef4cad22f5f15f";
        private const string CONTENT_VALID_HMACSHA256 = "4cff476bb1ca6e03f5368afde541ff206877eb5651e7e86581a27bb1eeb17d19";
        private const string CONTENT_INVALID_HMACSHA256 = "4cff476bb1ca6e03f5368afde541ff206877eb5651e7e86581a27bb1eeb17d1a";
        private const string CONTENT_VALID_HMACSHA384 = "f5d55d2b0234153e5a1775004e6c0610f695217aea619e02204dfa7e86b9bf4d633f8800bb65cdd743f64e9ce8c0bb72";
        private const string CONTENT_INVALID_HMACSHA384 = "f5d55d2b0234153e5a1775004e6c0610f695217aea619e02204dfa7e86b9bf4d633f8800bb65cdd743f64e9ce8c0bb73";
        private const string CONTENT_VALID_HMACSHA512 = "4a0e95a159a83675989f1433c98fc2de4fb9db534450f2db2bc02493632f82e2597cc6cf6be833efeefe15a2f8ca253d9e4726fdddca02278a79c8ded4c5f471";
        private const string CONTENT_INVALID_HMACSHA512 = "4a0e95a159a83675989f1433c98fc2de4fb9db534450f2db2bc02493632f82e2597cc6cf6be833efeefe15a2f8ca253d9e4726fdddca02278a79c8ded4c5f472";
        #endregion

        #region Prepare SUT
        private WebSubRequestServices PrepareWebSubRequestServices(
            string subscriptionId = WEBHOOK_ID,
            WebSubSubscriptionState subscriptionState = WebSubSubscriptionState.SubscribeValidated,
            string secret = null)
        {
            return new WebSubRequestServices(new Dictionary<string, WebSubSubscription>
            {
                { subscriptionId, new WebSubSubscription { Id = subscriptionId, State = subscriptionState, Secret = secret } }
            });
        }

        private WebSubRequestServices PrepareWebSubRequestServicesWithSecret(
            string subscriptionId = WEBHOOK_ID,
            WebSubSubscriptionState subscriptionState = WebSubSubscriptionState.SubscribeValidated)
        {
            return new WebSubRequestServices(new Dictionary<string, WebSubSubscription>
            {
                { subscriptionId, new WebSubSubscription { Id = subscriptionId, State = subscriptionState, Secret = SECRET } }
            });
        }

        private ResourceExecutingContext PrepareWebSubExecutingContext(
            string method,
            string id = WEBHOOK_ID,
            string content = null,
            IServiceProvider requestServices = null)
        {
            RouteData routeData = new RouteData();
            if (!String.IsNullOrWhiteSpace(id))
            {
                routeData.Values.Add("id", id);
            }

            Stream body = Stream.Null;
            if (content != null)
            {
                body = new MemoryStream();

                StreamWriter bodyWriter = new StreamWriter(body);
                bodyWriter.Write(content);
                bodyWriter.Flush();

                body.Position = 0;
            }

            return new ResourceExecutingContext(new ActionContext()
            {
                ActionDescriptor = new ActionDescriptor(),
                HttpContext = new WebSubHttpContext(method, body, requestServices ?? PrepareWebSubRequestServices()),
                RouteData = routeData
            }, new List<IFilterMetadata>(), new List<IValueProviderFactory>());
        }

        private ResourceExecutingContext PrepareContentDistributionExecutingContext(
            string id = WEBHOOK_ID,
            string content = null,
            IServiceProvider requestServices = null)
        {
            return PrepareWebSubExecutingContext(HttpMethods.Post, id, content, requestServices);
        }

        private ResourceExecutingContext PrepareAuthenticatedContentDistributionExecutingContext(string algorithm, string hash)
        {
            ResourceExecutingContext authenticatedContentDistributionExecutingContext = PrepareWebSubExecutingContext(HttpMethods.Post, WEBHOOK_ID, CONTENT, PrepareWebSubRequestServicesWithSecret(WEBHOOK_ID, WebSubSubscriptionState.SubscribeValidated));

            authenticatedContentDistributionExecutingContext.HttpContext.Request.Headers.Add("X-Hub-Signature", $"{algorithm}={hash}");

            return authenticatedContentDistributionExecutingContext;
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
        public async Task OnResourceExecutionAsync_WebSubRequestWithoutId_SetsNotFoundResult(string httpMethod)
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
        public async Task OnResourceExecutionAsync_WebSubRequestWithoutMatchingId_SetsNotFoundResult(string httpMethod)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareWebSubExecutingContext(httpMethod, id: OTHER_WEBHOOK_ID);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookSecurityFilter webSubWebHookSecurityFilter = PrepareWebSubWebHookSecurityFilter();

            await webSubWebHookSecurityFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<NotFoundResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async Task OnResourceExecutionAsync_IntentVerificationRequest_SetsSubscriptionHttpContextItem()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareWebSubExecutingContext(HttpMethods.Get);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookSecurityFilter webSubWebHookSecurityFilter = PrepareWebSubWebHookSecurityFilter();

            await webSubWebHookSecurityFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<WebSubSubscription>(resourceExecutingContext.HttpContext.Items[HTTP_CONTEXT_ITEMS_SUBSCRIPTION_KEY]);
        }

        [Fact]
        public async Task OnResourceExecutionAsync_IntentVerificationRequest_CallsNext()
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
        public async Task OnResourceExecutionAsync_ContentDistributionRequestForSubscriptionStateDifferentThanSubscribeValidated_SetsNotFoundResult(WebSubSubscriptionState webSubSubscriptionState)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareContentDistributionExecutingContext(requestServices: PrepareWebSubRequestServices(subscriptionState: webSubSubscriptionState));
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookSecurityFilter webSubWebHookSecurityFilter = PrepareWebSubWebHookSecurityFilter();

            await webSubWebHookSecurityFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<NotFoundResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async Task OnResourceExecutionAsync_ContentDistributionRequestForValidatedSubscriptionWithSecret_SetsBadRequestResult()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareContentDistributionExecutingContext(requestServices: PrepareWebSubRequestServicesWithSecret());
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookSecurityFilter webSubWebHookSecurityFilter = PrepareWebSubWebHookSecurityFilter();

            await webSubWebHookSecurityFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<BadRequestObjectResult>(resourceExecutingContext.Result);
        }

        [Fact]
        public async Task OnResourceExecutionAsync_ContentDistributionRequestForValidatedSubscriptionWithoutSecret_SetsSubscriptionHttpContextItem()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareContentDistributionExecutingContext();
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookSecurityFilter webSubWebHookSecurityFilter = PrepareWebSubWebHookSecurityFilter();

            await webSubWebHookSecurityFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<WebSubSubscription>(resourceExecutingContext.HttpContext.Items[HTTP_CONTEXT_ITEMS_SUBSCRIPTION_KEY]);
        }

        [Fact]
        public async Task OnResourceExecutionAsync_ContentDistributionRequestForValidatedSubscriptionWithoutSecret_CallsNext()
        {
            ResourceExecutingContext resourceExecutingContext = PrepareContentDistributionExecutingContext();
            Mock<ResourceExecutionDelegate> resourceExecutionDelegateMock = new Mock<ResourceExecutionDelegate>();

            WebSubWebHookSecurityFilter webSubWebHookSecurityFilter = PrepareWebSubWebHookSecurityFilter();

            await webSubWebHookSecurityFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegateMock.Object);

            resourceExecutionDelegateMock.Verify(m => m(), Times.Once);
        }

        [Theory]
        [InlineData("sha1", CONTENT_INVALID_HMACSHA1)]
        [InlineData("sha256", CONTENT_INVALID_HMACSHA256)]
        [InlineData("sha384", CONTENT_INVALID_HMACSHA384)]
        [InlineData("sha512", CONTENT_INVALID_HMACSHA512)]
        public async Task OnResourceExecutionAsync_AuthenticatedContentDistributionRequestWithInvalidHashForValidatedSubscriptionWithSecret_SetsBadRequestResult(string algorithm, string hash)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareAuthenticatedContentDistributionExecutingContext(algorithm, hash);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookSecurityFilter webSubWebHookSecurityFilter = PrepareWebSubWebHookSecurityFilter();

            await webSubWebHookSecurityFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<BadRequestObjectResult>(resourceExecutingContext.Result);
        }

        [Theory]
        [InlineData("sha1", CONTENT_VALID_HMACSHA1)]
        [InlineData("sha256", CONTENT_VALID_HMACSHA256)]
        [InlineData("sha384", CONTENT_VALID_HMACSHA384)]
        [InlineData("sha512", CONTENT_VALID_HMACSHA512)]
        public async Task OnResourceExecutionAsync_AuthenticatedContentDistributionRequestWithValidHashForValidatedSubscriptionWithSecret_SetsSubscriptionHttpContextItem(string algorithm, string hash)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareAuthenticatedContentDistributionExecutingContext(algorithm, hash);
            ResourceExecutionDelegate resourceExecutionDelegate = () => Task.FromResult<ResourceExecutedContext>(null);

            WebSubWebHookSecurityFilter webSubWebHookSecurityFilter = PrepareWebSubWebHookSecurityFilter();

            await webSubWebHookSecurityFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegate);

            Assert.IsType<WebSubSubscription>(resourceExecutingContext.HttpContext.Items[HTTP_CONTEXT_ITEMS_SUBSCRIPTION_KEY]);
        }

        [Theory]
        [InlineData("sha1", CONTENT_VALID_HMACSHA1)]
        [InlineData("sha256", CONTENT_VALID_HMACSHA256)]
        [InlineData("sha384", CONTENT_VALID_HMACSHA384)]
        [InlineData("sha512", CONTENT_VALID_HMACSHA512)]
        public async Task OnResourceExecutionAsync_AuthenticatedContentDistributionRequestWithValidHashForValidatedSubscriptionWithSecret_CallsNext(string algorithm, string hash)
        {
            ResourceExecutingContext resourceExecutingContext = PrepareAuthenticatedContentDistributionExecutingContext(algorithm, hash);
            Mock<ResourceExecutionDelegate> resourceExecutionDelegateMock = new Mock<ResourceExecutionDelegate>();

            WebSubWebHookSecurityFilter webSubWebHookSecurityFilter = PrepareWebSubWebHookSecurityFilter();

            await webSubWebHookSecurityFilter.OnResourceExecutionAsync(resourceExecutingContext, resourceExecutionDelegateMock.Object);

            resourceExecutionDelegateMock.Verify(m => m(), Times.Once);
        }
        #endregion
    }
}
