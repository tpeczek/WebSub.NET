using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WebSub.AspNetCore.Services;

namespace Demo.AspNetCore.WebSub.Subscriber.Services
{
    internal class MemoryWebSubSubscriptionsStore : WebSubSubscriptionStoreBase
    {
        #region Fields
        private static readonly ConcurrentDictionary<string, WebSubSubscription> _store = new ConcurrentDictionary<string, WebSubSubscription>();
        #endregion

        #region Constructor
        static MemoryWebSubSubscriptionsStore()
        {
            _store.TryAdd("73481a8e-c9ee-4ec4-89e3-b25b3179ae92", new WebSubSubscription
            {
                Id = "73481a8e-c9ee-4ec4-89e3-b25b3179ae92",
                State = WebSubSubscriptionState.SubscribeRequested,
                CallbackUrl = "https://localhost:5001/api/webhooks/incoming/websub/73481a8e-c9ee-4ec4-89e3-b25b3179ae92",
                TopicUrl = "https://websub.rocks/blog/100/yikxg9xINdT3Woe0lbNo"
            });
        }

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
