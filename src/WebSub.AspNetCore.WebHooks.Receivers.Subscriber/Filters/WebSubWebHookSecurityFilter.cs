using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebSub.AspNetCore.Services;

namespace WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters
{
    /// <summary>
    /// An <see cref="IAsyncResourceFilter"/> that verifies the subscription and handles authenticated content distribution.
    /// </summary>
    internal class WebSubWebHookSecurityFilter : WebHookVerifySignatureFilter, IAsyncResourceFilter
    {
        #region Properties
        /// <summary>
        /// Gets the case-insensitive name of the WebHook generator that this receiver supports.
        /// </summary>
        public override string ReceiverName => WebSubConstants.ReceiverName;
        #endregion

        #region Constructor
        /// <summary>
        /// Instantiates a new <see cref="WebSubWebHookSecurityFilter"/> instance.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
        /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public WebSubWebHookSecurityFilter(IConfiguration configuration, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
            : base(configuration, hostingEnvironment, loggerFactory)
        { }
        #endregion

        #region Methods
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            IActionResult secureConnectionCheckResult = EnsureSecureConnection(ReceiverName, context.HttpContext.Request);
            if (secureConnectionCheckResult != null)
            {
                context.Result = secureConnectionCheckResult;
                return;
            }

            if (!context.RouteData.TryGetWebHookReceiverId(out string subscriptionId))
            {
                context.Result = new NotFoundResult();
                return;
            }

            WebSubSubscription subscription = await RetrieveWebSubSubscriptionAsync(context, subscriptionId);
            if (subscription == null)
            {
                context.Result = new NotFoundResult();
                return;
            }

            if (HttpMethods.IsPost(context.HttpContext.Request.Method))
            {
                IActionResult contentDistributionRequestVerificationResult = VerifyContentDistributionRequest(subscription);
                if (contentDistributionRequestVerificationResult != null)
                {
                    context.Result = contentDistributionRequestVerificationResult;
                    return;
                }
            }

            context.HttpContext.Items[WebSubConstants.HTTP_CONTEXT_ITEMS_SUBSCRIPTION_KEY] = subscription;

            await next();
        }

        private static Task<WebSubSubscription> RetrieveWebSubSubscriptionAsync(ResourceExecutingContext context, string subscriptionId)
        {
            IWebSubSubscriptionsStore subscriptionsStore = context.HttpContext.RequestServices.GetRequiredService<IWebSubSubscriptionsStore>();

            return subscriptionsStore.RetrieveAsync(subscriptionId);
        }
        
        private static IActionResult VerifyContentDistributionRequest(WebSubSubscription subscription)
        {
            IActionResult verificationResult = null;

            if (subscription.State != WebSubSubscriptionState.SubscribeValidated)
            {
                verificationResult = new NotFoundResult();
            }

            return verificationResult;
        }
        #endregion
    }
}
