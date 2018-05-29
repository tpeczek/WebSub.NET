using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WebSub.Net.Http.Subscriber.Discovery
{
    internal class WebSubDiscoverer
    {
        #region Fields
        private const string LINK_HEADER = "Link";

        private readonly HttpClient _httpClient;
        #endregion

        #region Constructors
        public WebSubDiscoverer(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        #endregion

        #region Methods
        public async Task<WebSubDiscovery> Discover(string requestUri, CancellationToken cancellationToken)
        {
            HttpResponseMessage discoveryRequestResponse = await _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            return WebLinkParser.ParseWebLinkHeaders(discoveryRequestResponse.Headers.GetValues(LINK_HEADER));
        }
        #endregion
    }
}
