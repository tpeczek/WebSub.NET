using System.Threading;
using System.Threading.Tasks;

namespace WebSub.Net.Http.Subscriber.Discovery
{
    internal interface IWebSubDiscoverer
    {
        Task<WebSubDiscovery> DiscoverAsync(string requestUri, CancellationToken cancellationToken);
    }
}
