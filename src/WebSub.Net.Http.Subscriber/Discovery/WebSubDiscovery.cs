using System.Collections.Generic;

namespace WebSub.Net.Http.Subscriber.Discovery
{
    internal struct WebSubDiscovery
    {
        public string TopicUrl { get; set; }

        public List<string> HubsUrls { get; set; }
    }
}
