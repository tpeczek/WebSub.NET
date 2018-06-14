using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using WebSub.Net.Http.Subscriber.Discovery;
using Test.WebSub.Net.Http.Subscriber.WebSubRocks.Infrastructure;

namespace Test.WebSub.Net.Http.Subscriber.WebSubRocks
{
    public class WebSubDiscovererTests
    {
        #region Prepare SUT
        private WebSubDiscoverer PrepareWebSubRocksWebSubDiscoverer()
        {
            HttpClient webSubRocksHttpClient = new HttpClient(new WebSubRocksHttpMessageHandler());

            return new WebSubDiscoverer(webSubRocksHttpClient);
        }
        #endregion

        #region Tests
        [Fact]
        public async void Discover_InvalidDiscoveryUrl_ThrowsWebSubDiscoveryException()
        {
            WebSubDiscoverer webSubRocksWebSubDiscoverer = PrepareWebSubRocksWebSubDiscoverer();

            await Assert.ThrowsAsync<WebSubDiscoveryException>(() => webSubRocksWebSubDiscoverer.DiscoverAsync(WebSubRocksConstants.INVALID_DISCOVERY_URL, CancellationToken.None));
        }

        [Fact]
        public async void Discover_HttpHeaderDiscoveryUrl_DiscoversSingleHub()
        {
            WebSubDiscoverer webSubRocksWebSubDiscoverer = PrepareWebSubRocksWebSubDiscoverer();

            WebSubDiscoveredUrls webSubRocksWebSubDiscovery = await webSubRocksWebSubDiscoverer.DiscoverAsync(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, CancellationToken.None);

            Assert.Single(webSubRocksWebSubDiscovery.Hubs);
        }

        [Fact]
        public async Task Discover_HttpHeaderDiscoveryUrl_DiscoversCorrectHub()
        {
            WebSubDiscoverer webSubRocksWebSubDiscoverer = PrepareWebSubRocksWebSubDiscoverer();

            WebSubDiscoveredUrls webSubRocksWebSubDiscovery = await webSubRocksWebSubDiscoverer.DiscoverAsync(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, CancellationToken.None);

            Assert.Contains(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_HUB_URL, webSubRocksWebSubDiscovery.Hubs);
        }

        [Fact]
        public async Task Discover_HttpHeaderDiscoveryUrl_DiscoversCorrectTopic()
        {
            WebSubDiscoverer webSubRocksWebSubDiscoverer = PrepareWebSubRocksWebSubDiscoverer();

            WebSubDiscoveredUrls webSubRocksWebSubDiscovery = await webSubRocksWebSubDiscoverer.DiscoverAsync(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, CancellationToken.None);

            Assert.Equal(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_TOPIC_URL, webSubRocksWebSubDiscovery.Topic);
        }
        #endregion
    }
}
