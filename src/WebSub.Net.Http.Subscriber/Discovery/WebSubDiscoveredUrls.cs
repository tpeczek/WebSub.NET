using System.Collections.Generic;

namespace WebSub.Net.Http.Subscriber.Discovery
{
    /// <summary>
    /// URLs resulting from WebSub discovery.
    /// </summary>
    public struct WebSubDiscoveredUrls
    {
        #region Fields
        private List<string> _hubs;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the canonical URL for discovered topic.
        /// </summary>
        public string Topic { get; internal set; }

        /// <summary>
        /// Gets the URLs for discovered hubs.
        /// </summary>
        public IEnumerable<string> Hubs { get { return _hubs; } }
        #endregion

        #region Methods
        internal void AddHub(string url)
        {
            if (_hubs == null)
            {
                _hubs = new List<string>();
            }
            _hubs.Add(url);
        }
        #endregion
    }
}
