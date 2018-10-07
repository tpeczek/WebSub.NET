using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using WebSub.WebHooks.Receivers.Subscriber;
using WebSub.WebHooks.Receivers.Subscriber.Services;

namespace Demo.AspNetCore.WebSub.Subscriber.Services
{
    internal class MemoryWebSubSubscriptionsStore : IWebSubSubscriptionsStore
    {
        #region Fields
        private static readonly ConcurrentDictionary<string, WebSubSubscription> _store = new ConcurrentDictionary<string, WebSubSubscription>();
        #endregion

        #region Constructor
        public MemoryWebSubSubscriptionsStore()
        { }
        #endregion

        #region Methods
        public Task<WebSubSubscription> CreateAsync()
        {
            return CreateAsync(CancellationToken.None);
        }

        public Task<WebSubSubscription> CreateAsync(CancellationToken cancellationToken)
        {
            WebSubSubscription subscription = new WebSubSubscription
            {
                Id = Guid.NewGuid().ToString("D"),
                State = WebSubSubscriptionState.Created
            };

            if (!_store.TryAdd(subscription.Id, subscription))
            {
                throw new Exception("Error while creating subscription");
            }

            return Task.FromResult(subscription);
        }

        public Task RemoveAsync(string id)
        {
            return RemoveAsync(id, CancellationToken.None);
        }

        public Task RemoveAsync(string id, CancellationToken cancellationToken)
        {
            _store.TryRemove(id, out _);

            return Task.CompletedTask;
        }

        public Task RemoveAsync(WebSubSubscription subscription)
        {
            return RemoveAsync(subscription, CancellationToken.None);
        }

        public Task RemoveAsync(WebSubSubscription subscription, CancellationToken cancellationToken)
        {
            if (subscription != null)
            {
                _store.TryRemove(subscription.Id, out _);
            }

            return Task.CompletedTask;
        }

        public Task<WebSubSubscription> RetrieveAsync(string id)
        {
            return RetrieveAsync(id, CancellationToken.None);
        }

        public Task<WebSubSubscription> RetrieveAsync(string id, CancellationToken cancellationToken)
        {
            _store.TryGetValue(id, out WebSubSubscription subscription);

            return Task.FromResult(subscription);
        }

        public Task UpdateAsync(WebSubSubscription subscription)
        {
            return UpdateAsync(subscription, CancellationToken.None);
        }

        public Task UpdateAsync(WebSubSubscription webSubSubscription, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        #endregion
    }
}
