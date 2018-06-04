using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebSub.Net.Http.Subscriber.Discovery;
using System.Globalization;

namespace WebSub.Net.Http.Subscriber
{
    /// <summary>
    /// A WebSub subscriber.
    /// </summary>
    public class WebSubSubscriber
    {
        #region Fields
        private const string HUB_MODE_PARAMETER_NAME = "hub.mode";
        private const string HUB_TOPIC_PARAMETER_NAME = "hub.topic";
        private const string HUB_CALLBACK_PARAMETER_NAME = "hub.callback";
        private const string HUB_LEASE_SECONDS_PARAMETER_NAME = "hub.lease_seconds";
        private const string HUB_SECRET_PARAMETER_NAME = "hub.secret";

        private const string HUB_MODE_SUBCRIBE = "subscribe";
        private const string HUB_MODE_UNSUBSCRIBE = "unsubscribe";

        private readonly HttpClient _httpClient;
        private readonly IWebSubDiscoverer _webSubDiscoverer;
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
            : this(httpClient, new WebSubDiscoverer(httpClient))
        { }

        internal WebSubSubscriber(HttpClient httpClient, IWebSubDiscoverer webSubDiscoverer)
        {
            _httpClient = httpClient;
            _webSubDiscoverer = webSubDiscoverer;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Discovers topic and hub URLs based on publisher's content URL and performs subscription request.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<WebSubSubscription> SubscribeAsync(WebSubSubscribeParameters parameters, CancellationToken cancellationToken)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            WebSubDiscovery discovery = await _webSubDiscoverer.DiscoverAsync(parameters.ContentUrl, cancellationToken);

            string subscriptionHubUrl = null;
            Dictionary<string, HttpResponseMessage> hubsResponses = new Dictionary<string, HttpResponseMessage>();

            FormUrlEncodedContent subscribeRequestContent = PrepareSubscribeRequestContent(discovery.TopicUrl, parameters);
            foreach (string hubUrl in discovery.HubsUrls)
            {
                HttpResponseMessage hubResponse = await _httpClient.PostAsync(hubUrl, subscribeRequestContent, cancellationToken);
                if (hubResponse.StatusCode == HttpStatusCode.Accepted)
                {
                    subscriptionHubUrl = hubUrl;
                    break;
                }
                hubsResponses.Add(hubUrl, hubResponse);
            }

            if (String.IsNullOrWhiteSpace(subscriptionHubUrl))
            {
                throw new WebSubSubscriptionException("None of discovered hubs have accepted the request.", hubsResponses);
            }

            return new WebSubSubscription(discovery.TopicUrl, subscriptionHubUrl);
        }

        private static FormUrlEncodedContent PrepareSubscribeRequestContent(string topicUrl, WebSubSubscribeParameters parameters)
        {
            List<KeyValuePair<string, string>> subscribeParameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(HUB_MODE_PARAMETER_NAME, HUB_MODE_SUBCRIBE),
                new KeyValuePair<string, string>(HUB_TOPIC_PARAMETER_NAME, topicUrl),
                new KeyValuePair<string, string>(HUB_CALLBACK_PARAMETER_NAME, parameters.CallbackUrl)
            };

            if (parameters.LeaseSeconds.HasValue)
            {
                subscribeParameters.Add(new KeyValuePair<string, string>(HUB_LEASE_SECONDS_PARAMETER_NAME, parameters.LeaseSeconds.Value.ToString(CultureInfo.InvariantCulture)));
            }

            if (!String.IsNullOrWhiteSpace(parameters.Secret))
            {
                subscribeParameters.Add(new KeyValuePair<string, string>(HUB_SECRET_PARAMETER_NAME, parameters.Secret));
            }

            return new FormUrlEncodedContent(subscribeParameters);
        }
        #endregion
    }
}
