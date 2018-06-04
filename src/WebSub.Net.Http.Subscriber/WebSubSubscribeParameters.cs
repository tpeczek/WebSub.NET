using System;

namespace WebSub.Net.Http.Subscriber
{
    /// <summary>
    /// A WebSub subscription parameters.
    /// </summary>
    public class WebSubSubscribeParameters
    {
        #region Fields
        private const int MAX_SECRET_LENGTH = 200;

        private int? _leaseSeconds;
        private string _secret;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the publisher's content URL where content distribution discovery request should be made.
        /// </summary>
        public string ContentUrl { get; }

        /// <summary>
        /// Gets the subscriber's callback URL where content distribution notifications should be delivered.
        /// </summary>
        public string CallbackUrl { get; }

        /// <summary>
        /// Gets or sets the number of seconds for which the subscriber would like to have the subscription active.
        /// </summary>
        public int? LeaseSeconds
        {
            get { return _leaseSeconds; }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Number of lease seconds must be a positive integer.");
                }
                _leaseSeconds = value;
            }
        }

        /// <summary>
        /// Gets or sets the cryptographically random unique secret string that will be used to compute an HMAC digest for authorized content distribution.
        /// </summary>
        public string Secret
        {
            get { return _secret; }

            set
            {
                if (String.IsNullOrWhiteSpace(value) || (value.Length >= MAX_SECRET_LENGTH))
                {
                    throw new ArgumentException($"The secret length must less than {MAX_SECRET_LENGTH}.", nameof(value));
                }
                _secret = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates new instance of <see cref="WebSubSubscribeParameters"/> class.
        /// </summary>
        /// <param name="contentUrl">The publisher's content URL where content distribution discovery request should be made.</param>
        /// <param name="callbackUrl">The subscriber's callback URL where content distribution notifications should be delivered.</param>
        public WebSubSubscribeParameters(string contentUrl, string callbackUrl)
        {
            if (String.IsNullOrWhiteSpace(contentUrl))
            {
                throw new ArgumentNullException(nameof(contentUrl));
            }
            ContentUrl = contentUrl;

            if (String.IsNullOrWhiteSpace(callbackUrl))
            {
                throw new ArgumentNullException(nameof(callbackUrl));
            }
            CallbackUrl = callbackUrl;
        }
        #endregion
    }
}
