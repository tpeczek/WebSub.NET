using System;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Metadata;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Methods to add services for the WebSub receiver.
    /// </summary>
    internal static class WebSubServiceCollectionSetup
    {
        /// <summary>
        /// Add services for the WebSub receiver.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to update.</param>
        public static void AddWebSubServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            WebHookMetadata.Register<WebSubMetadata>(services);
            services.TryAddSingleton<WebSubWebHookSecurityFilter>();
            services.TryAddSingleton<WebSubWebHookIntentVerificationFilter>();
        }
    }
}
