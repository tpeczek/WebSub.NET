using System;
using System.Net.Http;

namespace WebSub.Net.Http.Subscriber.Discovery
{
    /// <summary>
    /// An exception representing WebSub discovery failure.
    /// </summary>
    public class WebSubDiscoveryException : Exception
    {
        #region Properties
        /// <summary>
        /// Gets the response to the discovery request.
        /// </summary>
        public HttpResponseMessage DiscoveryResponse { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates new instance of <see cref="WebSubDiscoveryException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the current exception.</param>
        public WebSubDiscoveryException(string message)
            : base(message)
        { }

        /// <summary>
        /// Creates new instance of <see cref="WebSubDiscoveryException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the current exception.</param>
        /// <param name="discoveryResponse">The response to the discovery request.</param>
        public WebSubDiscoveryException(string message, HttpResponseMessage discoveryResponse)
            : this(message)
        {
            DiscoveryResponse = discoveryResponse;
        }
        #endregion
    }
}
