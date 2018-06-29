using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Metadata;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber.ModelBinding;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber.ApplicationModels;

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

            services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, WebSubBindingInfoProvider>());

            services.TryAddSingleton<WebSubWebHookSecurityFilter>();
            services.TryAddSingleton<WebSubWebHookIntentVerificationFilter>();
        }
    }
}
