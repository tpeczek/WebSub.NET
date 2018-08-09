using System;

namespace WebSub.WebHooks.Receivers.Subscriber.Services
{
    /// <summary>
    /// Represents a WebSub subscription.
    /// </summary>
    public class WebSubSubscription
    {
        /// <summary>
        /// Gets or sets the unique identifier of the subscription.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="WebSubSubscriptionState"/> of the subscription.
        /// </summary>
        public WebSubSubscriptionState State { get; set; }

        /// <summary>
        /// Gets or sets the canonical URL for the subscription topic.
        /// </summary>
        public string TopicUrl { get; set; }

        /// <summary>
        /// Gets or sets the hub URL.
        /// </summary>
        public string HubUrl { get; set; }

        /// <summary>
        /// Gets or sets the subscriber's callback URL where content distribution notifications should be delivered.
        /// </summary>
        public string CallbackUrl { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds that the subscription will stay active before expiring, measured from <see cref="VerificationRequestTimeStampUtc"/>.
        /// </summary>
        public int? LeaseSeconds { get; set; }

        /// <summary>
        /// Gets or sets the time stamp of verification request.
        /// </summary>
        public DateTime? VerificationRequestTimeStampUtc { get; set; }

        /// <summary>
        /// Gets or sets the cryptographically random unique secret string that will be used to compute an HMAC digest for authorized content distribution.
        /// </summary>
        public string Secret { get; set; }
    }
}
