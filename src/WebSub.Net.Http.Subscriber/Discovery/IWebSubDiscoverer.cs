using System.Threading;
using System.Threading.Tasks;

namespace WebSub.Net.Http.Subscriber.Discovery
{
    internal interface IWebSubDiscoverer
    {
        Task<WebSubDiscoveredUrls> DiscoverAsync(string requestUri, CancellationToken cancellationToken);
    }
}
