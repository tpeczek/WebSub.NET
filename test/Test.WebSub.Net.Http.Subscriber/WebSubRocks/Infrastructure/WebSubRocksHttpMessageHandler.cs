using System;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Test.WebSub.Net.Http.Subscriber.WebSubRocks.Infrastructure
{
    internal class WebSubRocksHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, Func<Task<HttpResponseMessage>>> _webSubRocksGetResponseMessages = new Dictionary<string, Func<Task<HttpResponseMessage>>>
        {
            { WebSubRocksConstants.INVALID_DISCOVERY_URL, GetInvalidDiscoveryResponseMessage },
            { WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, GetHttpHeaderDiscoveryResponseMessage }
        };

        private readonly Dictionary<string, Func<Task<HttpResponseMessage>>> _webSubRocksPostResponseMessages = new Dictionary<string, Func<Task<HttpResponseMessage>>>
        {
            { WebSubRocksConstants.HTTP_HEADER_DISCOVERY_HUB_URL, GetAcceptedResponseMessage }
        };

        private readonly Task<HttpResponseMessage> _notFoundResponseMessageTask = Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if ((request.Method == HttpMethod.Get) && _webSubRocksGetResponseMessages.ContainsKey(request.RequestUri.AbsoluteUri))
            {
                return _webSubRocksGetResponseMessages[request.RequestUri.AbsoluteUri]();
            }

            if ((request.Method == HttpMethod.Post) && _webSubRocksPostResponseMessages.ContainsKey(request.RequestUri.AbsoluteUri))
            {
                return _webSubRocksPostResponseMessages[request.RequestUri.AbsoluteUri]();
            }

            return _notFoundResponseMessageTask;
        }

        private static Task<HttpResponseMessage> GetInvalidDiscoveryResponseMessage()
        {
            HttpResponseMessage invalidDiscoveryResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            invalidDiscoveryResponseMessage.Content = new StringContent("<!DOCTYPE html><html lang=\"en\"><head><title>WebSub Rocks!</title></head><body></body></html>", Encoding.UTF8, "text/html");

            return Task.FromResult(invalidDiscoveryResponseMessage);
        }

        private static Task<HttpResponseMessage> GetHttpHeaderDiscoveryResponseMessage()
        {
            HttpResponseMessage httpHeaderDiscoveryResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            httpHeaderDiscoveryResponseMessage.Headers.Add("Link", $"<{WebSubRocksConstants.HTTP_HEADER_DISCOVERY_TOPIC_URL}>; rel=\"self\"");
            httpHeaderDiscoveryResponseMessage.Headers.Add("Link", $"<{WebSubRocksConstants.HTTP_HEADER_DISCOVERY_HUB_URL}>; rel=\"hub\"");

            httpHeaderDiscoveryResponseMessage.Content = new StringContent("<!DOCTYPE html><html lang=\"en\"><head><title>WebSub Rocks!</title></head><body></body></html>", Encoding.UTF8, "text/html");

            return Task.FromResult(httpHeaderDiscoveryResponseMessage);
        }

        private static Task<HttpResponseMessage> GetAcceptedResponseMessage()
        {
            HttpResponseMessage httpHeaderDiscoveryResponseMessage = new HttpResponseMessage(HttpStatusCode.Accepted);

            return Task.FromResult(httpHeaderDiscoveryResponseMessage);
        }
    }
}
