using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WebSub.WebHooks.Receivers.Subscriber.Services;

namespace WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Services.EntityFrameworkCore
{
    internal class WebSubSubscriptionsStore<TContext> : WebSubSubscriptionStoreBase where TContext : WebSubDbContext
    {
        #region Fields
        private readonly TContext _webSubDbContext;
        #endregion

        #region Constructor
        public WebSubSubscriptionsStore(TContext webSubDbContext, IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _webSubDbContext = webSubDbContext;
        }
        #endregion

        #region Methods
        public override async Task<WebSubSubscription> CreateAsync(CancellationToken cancellationToken)
        {
            WebSubSubscription subscription = CreateInitializedWebSubSubscription();

            _webSubDbContext.Subscriptions.Add(subscription);

            await _webSubDbContext.SaveChangesAsync(cancellationToken);

            return subscription;
        }

        public override async Task RemoveAsync(string id, CancellationToken cancellationToken)
        {
            WebSubSubscription subscription = await RetrieveAsync(id, cancellationToken);

            await RemoveAsync(subscription, cancellationToken);
        }

        public override Task RemoveAsync(WebSubSubscription subscription, CancellationToken cancellationToken)
        {
            _webSubDbContext.Subscriptions.Remove(subscription);

            return _webSubDbContext.SaveChangesAsync(cancellationToken);
        }

        public override Task<WebSubSubscription> RetrieveAsync(string id, CancellationToken cancellationToken)
        {
            return _webSubDbContext.Subscriptions.FindAsync(id, cancellationToken);
        }

        public override Task UpdateAsync(WebSubSubscription subscription, CancellationToken cancellationToken)
        {
            return _webSubDbContext.SaveChangesAsync(cancellationToken);
        }
        #endregion
    }
}
