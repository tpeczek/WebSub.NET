using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebSub.WebHooks.Receivers.Subscriber.Services;

namespace Test.WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters.Infrastructure
{
    internal class WebSubRequestServices : IServiceProvider
    {
        private class DictionaryWebSubSubscriptionsStore : IWebSubSubscriptionsStore
        {
            private readonly IDictionary<string, WebSubSubscription> _store;

            public DictionaryWebSubSubscriptionsStore(IDictionary<string, WebSubSubscription> store)
            {
                _store = store;
            }

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
                return RemoveAsync(id, CancellationToken.None);
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
                WebSubSubscription subscription = _store.ContainsKey(id) ? _store[id] : null;

                return Task.FromResult(subscription);
            }

            public Task UpdateAsync(WebSubSubscription subscription)
            {
                return UpdateAsync(subscription, CancellationToken.None);
            }

            public Task UpdateAsync(WebSubSubscription subscription, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

        private readonly  IWebSubSubscriptionsStore _webSubSubscriptionsStore;

        public WebSubRequestServices(IDictionary<string, WebSubSubscription> webSubSubscriptionsStore)
        {
            _webSubSubscriptionsStore = new DictionaryWebSubSubscriptionsStore(webSubSubscriptionsStore);
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IWebSubSubscriptionsStore))
            {
                return _webSubSubscriptionsStore;
            }
            else
            {
                return null;
            }
        }
    }
}
