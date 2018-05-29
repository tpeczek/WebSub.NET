using System;

namespace WebSub.Net.Http.Subscriber.Discovery
{
    /// <summary>
    /// An exception representing WebSub discovery failure.
    /// </summary>
    public class WebSubDiscoveryException : Exception
    {
        /// <summary>
        /// Creates new instance of <see cref="WebSubDiscoveryException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the current exception</param>
        public WebSubDiscoveryException(string message)
            : base(message)
        { }
    }
}
