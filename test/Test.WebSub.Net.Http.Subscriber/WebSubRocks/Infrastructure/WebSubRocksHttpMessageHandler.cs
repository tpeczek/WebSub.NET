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
            { WebSubRocksConstants.HTTP_HEADER_DISCOVERY_URL, GetHttpHeaderDiscoveryResponseMessage }
        };

        private readonly Task<HttpResponseMessage> _notFoundResponseMessageTask = Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if ((request.Method == HttpMethod.Get) && _webSubRocksGetResponseMessages.ContainsKey(request.RequestUri.AbsoluteUri))
            {
                return _webSubRocksGetResponseMessages[request.RequestUri.AbsoluteUri]();
            }

            return _notFoundResponseMessageTask;
        }

        private static Task<HttpResponseMessage> GetHttpHeaderDiscoveryResponseMessage()
        {
            HttpResponseMessage httpHeaderDiscoveryResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            httpHeaderDiscoveryResponseMessage.Headers.Add("Link", $"<{WebSubRocksConstants.HTTP_HEADER_DISCOVERY_TOPIC}>; rel=\"self\"");
            httpHeaderDiscoveryResponseMessage.Headers.Add("Link", $"<{WebSubRocksConstants.HTTP_HEADER_DISCOVERY_HUB}>; rel=\"hub\"");

            httpHeaderDiscoveryResponseMessage.Content = new StringContent("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\" /><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge,chrome=1\" /><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, maximum-scale=1.0\"><title>WebSub Rocks!</title><link href=\"/assets/semantic.min.css\" rel=\"stylesheet\"><link href=\"/assets/style.css\" rel=\"stylesheet\"><script src=\"/assets/jquery-1.11.3.min.js\"></script><script src=\"/assets/semantic.min.js\"></script><script src=\"/assets/common.js\"></script></head><body><div class=\"ui top fixed menu\"><a class=\"item\" href=\"/\"><img src=\"/assets/websub-rocks-icon.png\"></a><a class=\"item\" href=\"/\">Home</a><a class=\"item\" href=\"/publisher\">Publisher</a><a class=\"item\" href=\"/subscriber\">Subscriber</a><a class=\"item\" href=\"/hub\">Hub</a></div><div class=\"single-column\"><div id=\"subscriber-post-list\" class=\"h-feed\"><span class=\"p-name hidden\">WebSub.rocks Test 100</span><section class=\"content h-entry\" id=\"quote-0\"><div class=\"e-content p-name\">Don’t let your design resist your readers. Don’t let it stand in the way of what they want to do: read.</div><div class=\"p-author h-card\">Steve Krug</div><a href=\"#quote-0\" class=\"u-url\"><time class=\"dt-published\" datetime=\"2018-04-12T17:24:18+00:00\">April 12, 2018 5:24pm</time></a></section><section class=\"content h-entry\" id=\"quote-1\"><div class=\"e-content p-name\">A lot of companies have tried to support designers by giving them &#8220;a seat at the table.&#8221; What this usually means in practice, however, is that a designer is sitting at the table well after the important product decisions that influence the design have been made. This is usually where complicated and muddy designs are born.</div><div class=\"p-author h-card\">Rebekah Cox</div><a href=\"#quote-1\" class=\"u-url\"><time class=\"dt-published\" datetime=\"2018-04-12T17:24:18+00:00\">April 12, 2018 5:24pm</time></a></section><section class=\"content h-entry\" id=\"quote-2\"><div class=\"e-content p-name\">And so we have an issue. A classic impasse. Management wants process. Developers want to build something that works. Designers want to spend time thinking about use cases. What do we do?</div><div class=\"p-author h-card\">Ben Bleikamp</div><a href=\"#quote-2\" class=\"u-url\"><time class=\"dt-published\" datetime=\"2018-04-12T17:24:18+00:00\">April 12, 2018 5:24pm</time></a></section></div></div></body></html>", Encoding.UTF8, "text/html");

            return Task.FromResult(httpHeaderDiscoveryResponseMessage);
        }
    }
}
