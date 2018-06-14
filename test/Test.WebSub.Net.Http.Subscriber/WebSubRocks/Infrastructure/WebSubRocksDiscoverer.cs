using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebSub.Net.Http.Subscriber.Discovery;

namespace Test.WebSub.Net.Http.Subscriber.WebSubRocks.Infrastructure
{
    internal class WebSubRocksDiscoverer : IWebSubDiscoverer
    {
        private readonly Dictionary<string, Func<Task<WebSubDiscoveredUrls>>> _webSubRocksDiscoveries = new Dictionary<string, Func<Task<WebSubDiscoveredUrls>>>
        {
            { WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, GetHttpHeaderWebSubDiscovery }
        };

        public Task<WebSubDiscoveredUrls> DiscoverAsync(string requestUri, CancellationToken cancellationToken)
        {
            if (_webSubRocksDiscoveries.ContainsKey(requestUri))
            {
                return _webSubRocksDiscoveries[requestUri]();
            }

            throw new WebSubDiscoveryException("The discovery mechanism haven't identified required URLs.");
        }

        private static Task<WebSubDiscoveredUrls> GetHttpHeaderWebSubDiscovery()
        {
            WebSubDiscoveredUrls webSubDiscovery = new WebSubDiscoveredUrls
            {
                Topic = WebSubRocksConstants.HTTP_HEADER_DISCOVERY_TOPIC_URL
            };
            webSubDiscovery.AddHub(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_HUB_URL);

            return Task.FromResult(webSubDiscovery);
        }
    }
}
