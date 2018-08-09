using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WebSub.WebHooks.Receivers.Subscriber.Services;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Services;

namespace Demo.AspNetCore.WebSub.Subscriber.Services
{
    internal class MemoryWebSubSubscriptionsStore : WebSubSubscriptionStoreBase
    {
        #region Fields
        private static readonly ConcurrentDictionary<string, WebSubSubscription> _store = new ConcurrentDictionary<string, WebSubSubscription>();
        #endregion

        #region Constructor
        public MemoryWebSubSubscriptionsStore(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        { }
        #endregion

        #region Methods
        public override Task<WebSubSubscription> CreateAsync(CancellationToken cancellationToken)
        {
            WebSubSubscription subscription = CreateInitializedWebSubSubscription();

            if (!_store.TryAdd(subscription.Id, subscription))
            {
                throw new Exception("Error while creating subscription");
            }

            return Task.FromResult(subscription);
        }

        public override Task RemoveAsync(string id, CancellationToken cancellationToken)
        {
            _store.TryRemove(id, out _);

            return Task.CompletedTask;
        }

        public override Task RemoveAsync(WebSubSubscription subscription, CancellationToken cancellationToken)
        {
            if (subscription != null)
            {
                _store.TryRemove(subscription.Id, out _);
            }

            return Task.CompletedTask;
        }

        public override Task<WebSubSubscription> RetrieveAsync(string id, CancellationToken cancellationToken)
        {
            _store.TryGetValue(id, out WebSubSubscription subscription);

            return Task.FromResult(subscription);
        }

        public override Task UpdateAsync(WebSubSubscription webSubSubscription, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        #endregion
    }
}
