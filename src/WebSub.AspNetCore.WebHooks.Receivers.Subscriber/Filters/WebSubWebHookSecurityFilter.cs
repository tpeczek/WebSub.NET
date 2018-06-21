using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

            if (context.RouteData.TryGetWebHookReceiverId(out string subscriptionId))
            {
                await next();
            }

            context.Result = new NotFoundResult();
        }
        #endregion
    }
}
