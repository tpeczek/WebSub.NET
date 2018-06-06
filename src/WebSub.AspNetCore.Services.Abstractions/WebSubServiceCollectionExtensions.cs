using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebSub.AspNetCore.Services;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// The <see cref="IServiceCollection"/> extensions for adding WebSub related services.
    /// </summary>
    public static class WebSubServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="IWebSubSubscriptionsStore"/> service implementations based on <see cref="WebSubSubscriptionStoreBase"/>.
        /// </summary>
        /// <param name="services">The collection of service descriptors.</param>
        /// <returns>The collection of service descriptors.</returns>
        public static IServiceCollection AddWebSubSubscriptionStore<T>(this IServiceCollection services) where T : WebSubSubscriptionStoreBase
        {
            services.AddHttpContextAccessor();
            services.TryAddScoped<IWebSubSubscriptionsStore, T>();

            return services;
        }
    }
}
