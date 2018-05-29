using System;
using System.Collections.Generic;

namespace WebSub.Net.Http.Subscriber.Discovery
{
    internal struct WebSubDiscovery
    {
        public bool Identified
        {
            get { return !String.IsNullOrWhiteSpace(Topic) && (Hubs != null) && (Hubs.Count > 0); }
        }

        public string Topic { get; set; }

        public List<string> Hubs { get; set; }
    }
}
