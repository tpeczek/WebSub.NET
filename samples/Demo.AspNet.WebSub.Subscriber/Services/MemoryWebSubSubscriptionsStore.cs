using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WebSub.WebHooks.Receivers.Subscriber;
using WebSub.WebHooks.Receivers.Subscriber.Services;

namespace Demo.AspNet.WebSub.Subscriber.Services
{
    internal class MemoryWebSubSubscriptionsStore : IWebSubSubscriptionsStore
    {
        #region Fields
        private static readonly Task _completedTask = Task.FromResult(true);
        private static readonly ConcurrentDictionary<string, WebSubSubscription> _store = new ConcurrentDictionary<string, WebSubSubscription>();
        #endregion

        #region Constructor
        public MemoryWebSubSubscriptionsStore()
        { }
        #endregion

        #region Methods
        public Task<WebSubSubscription> CreateAsync()
        {
            throw new NotImplementedException();
        }

        public Task<WebSubSubscription> CreateAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(WebSubSubscription subscription)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(WebSubSubscription subscription, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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

        public Task UpdateAsync(WebSubSubscription subscription, CancellationToken cancellationToken)
        {
            return _completedTask;
        }
        #endregion
    }
}