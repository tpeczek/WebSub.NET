using System;

namespace WebSub.Net.Http.Subscriber
{
    /// <summary>
    /// URLs to which the WebSub subscription has been made.
    /// </summary>
    public readonly struct WebSubSubscribedUrls
    {
        #region Properties
        /// <summary>
        /// Gets the canonical URL for the topic.
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// Gets the hub URL.
        /// </summary>
        public string Hub { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates new <see cref="WebSubSubscribedUrls"/>.
        /// </summary>
        /// <param name="topic">The canonical URL for the topic.</param>
        /// <param name="hub">The hub URL.</param>
        public WebSubSubscribedUrls(string topic, string hub)
            : this()
        {
            if (String.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentNullException(nameof(topic));
            }
            Topic = topic;

            if (String.IsNullOrWhiteSpace(hub))
            {
                throw new ArgumentNullException(nameof(hub));
            }
            Hub = hub;
        }
        #endregion
    }
}
