using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using WebSub.Net.Http.Subscriber.Discovery;
using Test.WebSub.Net.Http.Subscriber.WebSubRocks.Infrastructure;


namespace Test.WebSub.Net.Http.Subscriber.WebSubRocks
{
    public class DiscoveryTests
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
        public async void DiscoverFromHttpHeader_Discovers()
        {
            WebSubDiscoverer webSubRocksWebSubDiscoverer = PrepareWebSubRocksWebSubDiscoverer();

            WebSubDiscovery webSubRocksWebSubDiscovery = await webSubRocksWebSubDiscoverer.Discover(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, CancellationToken.None);

            Assert.True(webSubRocksWebSubDiscovery.Identified);
        }

        [Fact]
        public async void DiscoverFromHttpHeader_DiscoversSingleHub()
        {
            WebSubDiscoverer webSubRocksWebSubDiscoverer = PrepareWebSubRocksWebSubDiscoverer();

            WebSubDiscovery webSubRocksWebSubDiscovery = await webSubRocksWebSubDiscoverer.Discover(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, CancellationToken.None);

            Assert.Single(webSubRocksWebSubDiscovery.Hubs);
        }

        [Fact]
        public async Task DiscoverFromHttpHeader_DiscoversCorrectHub()
        {
            WebSubDiscoverer webSubRocksWebSubDiscoverer = PrepareWebSubRocksWebSubDiscoverer();

            WebSubDiscovery webSubRocksWebSubDiscovery = await webSubRocksWebSubDiscoverer.Discover(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, CancellationToken.None);

            Assert.Contains(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_HUB, webSubRocksWebSubDiscovery.Hubs);
        }

        [Fact]
        public async Task DiscoverFromHttpHeader_DiscoversCorrectTopic()
        {
            WebSubDiscoverer webSubRocksWebSubDiscoverer = PrepareWebSubRocksWebSubDiscoverer();

            WebSubDiscovery webSubRocksWebSubDiscovery = await webSubRocksWebSubDiscoverer.Discover(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, CancellationToken.None);

            Assert.Equal(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_TOPIC, webSubRocksWebSubDiscovery.Topic);
        }
        #endregion
    }
}
