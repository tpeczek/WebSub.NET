using System.Net.Http;
using WebSub.Net.Http.Subscriber.Discovery;

namespace WebSub.Net.Http.Subscriber
{
    /// <summary>
    /// A WebSub subscriber.
    /// </summary>
    public class WebSubSubscriber
    {
        #region Fields
        private readonly HttpClient _httpClient;
        private readonly WebSubDiscoverer _webSubDiscoverer;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates new instance of <see cref="WebSubSubscriber"/> class.
        /// </summary>
        public WebSubSubscriber()
            : this(new HttpClient())
        { }

        /// <summary>
        /// Creates new instance of <see cref="WebSubSubscriber"/> class.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance.</param>
        public WebSubSubscriber(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _webSubDiscoverer = new WebSubDiscoverer(_httpClient);
        }
        #endregion
    }
}
