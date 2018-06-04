using System;
using System.Net.Http;
using System.Collections.Generic;

namespace WebSub.Net.Http.Subscriber
{
    /// <summary>
    /// An exception representing WebSub subscribe/unsubscribe failure.
    /// </summary>
    public class WebSubSubscriptionException : Exception
    {
        #region Properties
        /// <summary>
        /// Gets the responses from hubs to which the request has been send.
        /// </summary>
        public IReadOnlyDictionary<string, HttpResponseMessage> HubsResponses { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates new instance of <see cref="WebSubSubscriptionException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the current exception.</param>
        public WebSubSubscriptionException(string message)
            : base(message)
        { }

        /// <summary>
        /// Creates new instance of <see cref="WebSubSubscriptionException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the current exception.</param>
        /// <param name="hubsResponses">The responses from hubs to which the request has been send.</param>
        public WebSubSubscriptionException(string message, IReadOnlyDictionary<string, HttpResponseMessage> hubsResponses)
            : this(message)
        {
            HubsResponses = hubsResponses;
        }
        #endregion
    }
}
