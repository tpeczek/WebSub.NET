using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using WebSub.WebHooks.Receivers.Subscriber.Services;

namespace Test.WebSub.AspNet.WebHooks.Receivers.Subscriber.WebHooks.Infrastructure
{
    internal class WebSubDependencyResolver : IDependencyResolver
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
                return Task.FromResult<Object>(null);
            }
        }

        private static readonly Type _webSubSubscriptionsStoreType = typeof(IWebSubSubscriptionsStore);

        private readonly IWebSubSubscriptionsStore _webSubSubscriptionsStore;

        public WebSubDependencyResolver()
            : this(new Dictionary<string, WebSubSubscription>())
        { }

        public WebSubDependencyResolver(IDictionary<string, WebSubSubscription> webSubSubscriptionsStore)
        {
            _webSubSubscriptionsStore = new DictionaryWebSubSubscriptionsStore(webSubSubscriptionsStore);
        }

        public IDependencyScope BeginScope()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == _webSubSubscriptionsStoreType)
            {
                return _webSubSubscriptionsStore;
            }

            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
