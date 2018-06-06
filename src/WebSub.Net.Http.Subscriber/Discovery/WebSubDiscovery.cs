using System.Collections.Generic;

namespace WebSub.Net.Http.Subscriber.Discovery
{
    /// <summary>
    /// A WebSub discovery.
    /// </summary>
    public struct WebSubDiscovery
    {
        #region Fields
        private List<string> _hubsUrls;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the canonical URL for discovered topic.
        /// </summary>
        public string TopicUrl { get; internal set; }

        /// <summary>
        /// Gets the URLs for discovered hubs.
        /// </summary>
        public IEnumerable<string> HubsUrls { get { return _hubsUrls; } }
        #endregion

        #region Methods
        internal void AddHubUrl(string url)
        {
            if (_hubsUrls == null)
            {
                _hubsUrls = new List<string>();
            }
            _hubsUrls.Add(url);
        }
        #endregion
    }
}
