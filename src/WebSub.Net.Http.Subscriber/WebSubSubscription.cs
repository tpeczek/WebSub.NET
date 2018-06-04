using System;

namespace WebSub.Net.Http.Subscriber
{
    /// <summary>
    /// A WebSub subscription.
    /// </summary>
    public struct WebSubSubscription
    {
        #region Properties
        /// <summary>
        /// Gets the canonical URL for the topic.
        /// </summary>
        public string TopicUrl { get; }

        /// <summary>
        /// Gets the hub URL.
        /// </summary>
        public string HubUrl { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates new <see cref="WebSubSubscription"/>.
        /// </summary>
        /// <param name="topicUrl">The canonical URL for the topic.</param>
        /// <param name="hubUrl">The hub URL.</param>
        public WebSubSubscription(string topicUrl, string hubUrl)
            : this()
        {
            if (String.IsNullOrWhiteSpace(topicUrl))
            {
                throw new ArgumentNullException(nameof(topicUrl));
            }
            TopicUrl = topicUrl;

            if (String.IsNullOrWhiteSpace(hubUrl))
            {
                throw new ArgumentNullException(nameof(hubUrl));
            }
            HubUrl = hubUrl;
        }
        #endregion
    }
}
