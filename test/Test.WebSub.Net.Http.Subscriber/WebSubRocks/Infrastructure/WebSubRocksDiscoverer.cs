using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebSub.Net.Http.Subscriber.Discovery;

namespace Test.WebSub.Net.Http.Subscriber.WebSubRocks.Infrastructure
{
    internal class WebSubRocksDiscoverer : IWebSubDiscoverer
    {
        private readonly Dictionary<string, Func<Task<WebSubDiscovery>>> _webSubRocksDiscoveries = new Dictionary<string, Func<Task<WebSubDiscovery>>>
        {
            { WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, GetHttpHeaderWebSubDiscovery }
        };

        public Task<WebSubDiscovery> DiscoverAsync(string requestUri, CancellationToken cancellationToken)
        {
            if (_webSubRocksDiscoveries.ContainsKey(requestUri))
            {
                return _webSubRocksDiscoveries[requestUri]();
            }

            throw new WebSubDiscoveryException("The discovery mechanism haven't identified required URLs.");
        }

        private static Task<WebSubDiscovery> GetHttpHeaderWebSubDiscovery()
        {
            WebSubDiscovery webSubDiscovery = new WebSubDiscovery
            {
                TopicUrl = WebSubRocksConstants.HTTP_HEADER_DISCOVERY_TOPIC_URL
            };
            webSubDiscovery.AddHubUrl(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_HUB_URL);

            return Task.FromResult(webSubDiscovery);
        }
    }
}
