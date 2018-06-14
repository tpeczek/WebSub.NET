using System.Net;
using System.Net.Http;
using System.Threading;
using System.Globalization;
using Moq;
using Xunit;
using WebSub.Net.Http.Subscriber;
using WebSub.Net.Http.Subscriber.Discovery;
using Test.WebSub.Net.Http.Subscriber.WebSubRocks.Infrastructure;

namespace Test.WebSub.Net.Http.Subscriber.WebSubRocks
{
    public class WebSubSubscriberTests
    {
        #region Fields
        private const string HUB_MODE_PARAMETER_NAME = "hub.mode";
        private const string HUB_TOPIC_PARAMETER_NAME = "hub.topic";
        private const string HUB_CALLBACK_PARAMETER_NAME = "hub.callback";
        private const string HUB_LEASE_SECONDS_PARAMETER_NAME = "hub.lease_seconds";
        private const string HUB_SECRET_PARAMETER_NAME = "hub.secret";

        private const string HUB_MODE_SUBCRIBE = "subscribe";
        private const string HUB_MODE_UNSUBCRIBE = "unsubscribe";

        private const int LEASE_SECONDS = 86400;
        private const string SECRET = "secret123";
        #endregion

        #region Prepare SUT
        private Mock<HttpClient> PrepareWebSubRocksHttpClientMock()
        {
            HttpClient webSubRocksHttpClient = new HttpClient(new WebSubRocksHttpMessageHandler());

            Mock<HttpClient> webSubRocksHttpClientMock = new Mock<HttpClient>();
            webSubRocksHttpClientMock.Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Returns((HttpRequestMessage request, CancellationToken cancellationToken) => webSubRocksHttpClient.SendAsync(request, cancellationToken));

            return webSubRocksHttpClientMock;
        }

        private Mock<IWebSubDiscoverer> PrepareWebSubRocksDiscovererMock()
        {
            IWebSubDiscoverer webSubRocksDiscoverer = new WebSubRocksDiscoverer();

            Mock<IWebSubDiscoverer> webSubRocksDiscovererMock = new Mock<IWebSubDiscoverer>();
            webSubRocksDiscovererMock.Setup(m => m.DiscoverAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns((string requestUri, CancellationToken cancellationToken) => webSubRocksDiscoverer.DiscoverAsync(requestUri, cancellationToken));

            return webSubRocksDiscovererMock;
        }

        private WebSubSubscriber PrepareWebSubRocksWebSubSubscriber(HttpClient webSubRocksHttpClient = null, IWebSubDiscoverer webSubRocksDiscoverer = null)
        {
            webSubRocksHttpClient = webSubRocksHttpClient ?? new HttpClient(new WebSubRocksHttpMessageHandler());
            webSubRocksDiscoverer = webSubRocksDiscoverer ?? new WebSubRocksDiscoverer();

            return new WebSubSubscriber(webSubRocksHttpClient, webSubRocksDiscoverer);
        }
        #endregion

        #region Tests
        [Fact]
        public async void Subscribe_CallsDiscoverWithContentUrl()
        {
            Mock<IWebSubDiscoverer> webSubRocksDiscovererMock = PrepareWebSubRocksDiscovererMock();
            WebSubSubscriber webSubSubscriber = PrepareWebSubRocksWebSubSubscriber(webSubRocksDiscoverer: webSubRocksDiscovererMock.Object);

            WebSubSubscribedUrls webSubSubscription = await webSubSubscriber.SubscribeAsync(new WebSubSubscribeParameters(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, WebSubRocksConstants.WEBHOOK_URL), CancellationToken.None);

            webSubRocksDiscovererMock.Verify(m => m.DiscoverAsync(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async void Subscribe_HttpHeaderDiscoveryUrl_PostsToHttpHeaderDiscoveryHubUrl()
        {
            Mock<HttpClient> webSubRocksHttpClientMock = PrepareWebSubRocksHttpClientMock();
            WebSubSubscriber webSubSubscriber = PrepareWebSubRocksWebSubSubscriber(webSubRocksHttpClientMock.Object);

            WebSubSubscribedUrls webSubSubscription = await webSubSubscriber.SubscribeAsync(new WebSubSubscribeParameters(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, WebSubRocksConstants.WEBHOOK_URL), CancellationToken.None);

            webSubRocksHttpClientMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(request => (request.Method == HttpMethod.Post) && (request.RequestUri.AbsoluteUri == WebSubRocksConstants.HTTP_HEADER_DISCOVERY_HUB_URL)), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async void Subscribe_HttpHeaderDiscoveryUrl_HubModeParameterEqualsSubscribe()
        {
            Mock<HttpClient> webSubRocksHttpClientMock = PrepareWebSubRocksHttpClientMock();
            WebSubSubscriber webSubSubscriber = PrepareWebSubRocksWebSubSubscriber(webSubRocksHttpClientMock.Object);

            WebSubSubscribedUrls webSubSubscription = await webSubSubscriber.SubscribeAsync(new WebSubSubscribeParameters(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, WebSubRocksConstants.WEBHOOK_URL), CancellationToken.None);

            webSubRocksHttpClientMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(request => request.Content.ReadAsStringAsync().Result.Contains($"{HUB_MODE_PARAMETER_NAME}={HUB_MODE_SUBCRIBE}")), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async void Subscribe_HttpHeaderDiscoveryUrl_HubTopicParameterEqualsHttpHeaderDiscoveryTopicUrl()
        {
            Mock<HttpClient> webSubRocksHttpClientMock = PrepareWebSubRocksHttpClientMock();
            WebSubSubscriber webSubSubscriber = PrepareWebSubRocksWebSubSubscriber(webSubRocksHttpClientMock.Object);

            WebSubSubscribedUrls webSubSubscription = await webSubSubscriber.SubscribeAsync(new WebSubSubscribeParameters(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, WebSubRocksConstants.WEBHOOK_URL), CancellationToken.None);

            webSubRocksHttpClientMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(request => request.Content.ReadAsStringAsync().Result.Contains($"{HUB_TOPIC_PARAMETER_NAME}={WebUtility.UrlEncode(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_TOPIC_URL)}")), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async void Subscribe_HttpHeaderDiscoveryUrl_HubCallbackParameterEqualsWebHookUrl()
        {
            Mock<HttpClient> webSubRocksHttpClientMock = PrepareWebSubRocksHttpClientMock();
            WebSubSubscriber webSubSubscriber = PrepareWebSubRocksWebSubSubscriber(webSubRocksHttpClientMock.Object);

            WebSubSubscribedUrls webSubSubscription = await webSubSubscriber.SubscribeAsync(new WebSubSubscribeParameters(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, WebSubRocksConstants.WEBHOOK_URL), CancellationToken.None);

            webSubRocksHttpClientMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(request => request.Content.ReadAsStringAsync().Result.Contains($"{HUB_CALLBACK_PARAMETER_NAME}={WebUtility.UrlEncode(WebSubRocksConstants.WEBHOOK_URL)}")), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async void Subscribe_HttpHeaderDiscoveryUrlNoLeaseSeconds_HubLeaseSecondsParameterNotPresent()
        {
            Mock<HttpClient> webSubRocksHttpClientMock = PrepareWebSubRocksHttpClientMock();
            WebSubSubscriber webSubSubscriber = PrepareWebSubRocksWebSubSubscriber(webSubRocksHttpClientMock.Object);

            WebSubSubscribedUrls webSubSubscription = await webSubSubscriber.SubscribeAsync(new WebSubSubscribeParameters(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, WebSubRocksConstants.WEBHOOK_URL), CancellationToken.None);

            webSubRocksHttpClientMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(request => request.Content.ReadAsStringAsync().Result.Contains($"{HUB_LEASE_SECONDS_PARAMETER_NAME}=")), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async void Subscribe_HttpHeaderDiscoveryUrlWithLeaseSeconds_HubLeaseSecondsParameterEqualsLeaseSeconds()
        {
            Mock<HttpClient> webSubRocksHttpClientMock = PrepareWebSubRocksHttpClientMock();
            WebSubSubscriber webSubSubscriber = PrepareWebSubRocksWebSubSubscriber(webSubRocksHttpClientMock.Object);

            WebSubSubscribedUrls webSubSubscription = await webSubSubscriber.SubscribeAsync(new WebSubSubscribeParameters(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, WebSubRocksConstants.WEBHOOK_URL) { LeaseSeconds = LEASE_SECONDS }, CancellationToken.None);

            webSubRocksHttpClientMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(request => request.Content.ReadAsStringAsync().Result.Contains($"{HUB_LEASE_SECONDS_PARAMETER_NAME}={LEASE_SECONDS.ToString(CultureInfo.InvariantCulture)}")), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async void Subscribe_HttpHeaderDiscoveryUrlNoSecret_HubSecretParameterNotPresent()
        {
            Mock<HttpClient> webSubRocksHttpClientMock = PrepareWebSubRocksHttpClientMock();
            WebSubSubscriber webSubSubscriber = PrepareWebSubRocksWebSubSubscriber(webSubRocksHttpClientMock.Object);

            WebSubSubscribedUrls webSubSubscription = await webSubSubscriber.SubscribeAsync(new WebSubSubscribeParameters(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, WebSubRocksConstants.WEBHOOK_URL), CancellationToken.None);

            webSubRocksHttpClientMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(request => request.Content.ReadAsStringAsync().Result.Contains($"{HUB_SECRET_PARAMETER_NAME}=")), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async void Subscribe_HttpHeaderDiscoveryUrlWithSecret_HubSecretParameterEqualsSecret()
        {
            Mock<HttpClient> webSubRocksHttpClientMock = PrepareWebSubRocksHttpClientMock();
            WebSubSubscriber webSubSubscriber = PrepareWebSubRocksWebSubSubscriber(webSubRocksHttpClientMock.Object);

            WebSubSubscribedUrls webSubSubscription = await webSubSubscriber.SubscribeAsync(new WebSubSubscribeParameters(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, WebSubRocksConstants.WEBHOOK_URL) { Secret = SECRET }, CancellationToken.None);

            webSubRocksHttpClientMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(request => request.Content.ReadAsStringAsync().Result.Contains($"{HUB_SECRET_PARAMETER_NAME}={SECRET}")), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async void Unsubscribe_HttpHeaderDiscoverySubscribedUrls_PostsToHttpHeaderDiscoveryHubUrl()
        {
            WebSubSubscribedUrls httpHeaderDiscoverySubscribedUrls = new WebSubSubscribedUrls(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_TOPIC_URL, WebSubRocksConstants.HTTP_HEADER_DISCOVERY_HUB_URL);
            Mock<HttpClient> webSubRocksHttpClientMock = PrepareWebSubRocksHttpClientMock();
            WebSubSubscriber webSubSubscriber = PrepareWebSubRocksWebSubSubscriber(webSubRocksHttpClientMock.Object);

            bool accepted = await webSubSubscriber.UnsubscribeAsync(httpHeaderDiscoverySubscribedUrls, WebSubRocksConstants.WEBHOOK_URL, CancellationToken.None);

            webSubRocksHttpClientMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(request => (request.Method == HttpMethod.Post) && (request.RequestUri.AbsoluteUri == WebSubRocksConstants.HTTP_HEADER_DISCOVERY_HUB_URL)), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async void Unsubscribe_HttpHeaderDiscoverySubscribedUrls_HubModeParameterEqualsUnsubscribe()
        {
            WebSubSubscribedUrls httpHeaderDiscoverySubscribedUrls = new WebSubSubscribedUrls(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_TOPIC_URL, WebSubRocksConstants.HTTP_HEADER_DISCOVERY_HUB_URL);
            Mock<HttpClient> webSubRocksHttpClientMock = PrepareWebSubRocksHttpClientMock();
            WebSubSubscriber webSubSubscriber = PrepareWebSubRocksWebSubSubscriber(webSubRocksHttpClientMock.Object);

            bool accepted = await webSubSubscriber.UnsubscribeAsync(httpHeaderDiscoverySubscribedUrls, WebSubRocksConstants.WEBHOOK_URL, CancellationToken.None);

            webSubRocksHttpClientMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(request => request.Content.ReadAsStringAsync().Result.Contains($"{HUB_MODE_PARAMETER_NAME}={HUB_MODE_UNSUBCRIBE}")), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async void Unsubscribe_HttpHeaderDiscoverySubscribedUrls_HubTopicParameterEqualsHttpHeaderDiscoveryTopicUrl()
        {
            WebSubSubscribedUrls httpHeaderDiscoverySubscribedUrls = new WebSubSubscribedUrls(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_TOPIC_URL, WebSubRocksConstants.HTTP_HEADER_DISCOVERY_HUB_URL);
            Mock<HttpClient> webSubRocksHttpClientMock = PrepareWebSubRocksHttpClientMock();
            WebSubSubscriber webSubSubscriber = PrepareWebSubRocksWebSubSubscriber(webSubRocksHttpClientMock.Object);

            bool accepted = await webSubSubscriber.UnsubscribeAsync(httpHeaderDiscoverySubscribedUrls, WebSubRocksConstants.WEBHOOK_URL, CancellationToken.None);

            webSubRocksHttpClientMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(request => request.Content.ReadAsStringAsync().Result.Contains($"{HUB_TOPIC_PARAMETER_NAME}={WebUtility.UrlEncode(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_TOPIC_URL)}")), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async void Unsubscribe_HttpHeaderDiscoverySubscribedUrls_HubCallbackParameterEqualsWebHookUrl()
        {
            WebSubSubscribedUrls httpHeaderDiscoverySubscribedUrls = new WebSubSubscribedUrls(WebSubRocksConstants.HTTP_HEADER_DISCOVERY_TOPIC_URL, WebSubRocksConstants.HTTP_HEADER_DISCOVERY_HUB_URL);
            Mock<HttpClient> webSubRocksHttpClientMock = PrepareWebSubRocksHttpClientMock();
            WebSubSubscriber webSubSubscriber = PrepareWebSubRocksWebSubSubscriber(webSubRocksHttpClientMock.Object);

            bool accepted = await webSubSubscriber.UnsubscribeAsync(httpHeaderDiscoverySubscribedUrls, WebSubRocksConstants.WEBHOOK_URL, CancellationToken.None);

            webSubRocksHttpClientMock.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(request => request.Content.ReadAsStringAsync().Result.Contains($"{HUB_CALLBACK_PARAMETER_NAME}={WebUtility.UrlEncode(WebSubRocksConstants.WEBHOOK_URL)}")), CancellationToken.None), Times.Once);
        }
        #endregion
    }
}
