using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.WebHooks;
using WebSub.WebHooks.Receivers.Subscriber;
using WebSub.WebHooks.Receivers.Subscriber.Services;
using WebSub.AspNet.WebHooks.Receivers.Subscriber.Extensions;

namespace WebSub.AspNet.WebHooks.Receivers.Subscriber.WebHooks
{
    /// <summary>
    /// Provides an <see cref="IWebHookReceiver"/> implementation which supports WebSub subscriber WebHooks.
    /// </summary>
    public class WebSubWebHookReceiver : WebHookReceiver
    {
        #region Fields
        internal const string RECEIVER_NAME = "websub";

        private const string MODE_QUERY_PARAMETER_NAME = "hub.mode";
        private const string TOPIC_QUERY_PARAMETER_NAME = "hub.topic";

        private const string MODE_DENIED = "denied";
        private const string MODE_SUBSCRIBE = "subscribe";
        private const string MODE_UNSUBSCRIBE = "unsubscribe";

        private const string INTENT_VERIFICATION_CHALLENGE_QUERY_PARAMETER_NAME = "hub.challenge";
        private const string INTENT_VERIFICATION_LEASE_SECONDS_QUERY_PARAMETER_NAME = "hub.lease_seconds";

        private const string INTENT_DENY_REASON_QUERY_PARAMETER_NAME = "hub.reason";

        internal const string SIGNATURE_HEADER_NAME = "X-Hub-Signature";
        internal const string SIGNATURE_HEADER_SHA1_KEY = "sha1";
        internal const string SIGNATURE_HEADER_SHA256_KEY = "sha256";
        internal const string SIGNATURE_HEADER_SHA384_KEY = "sha384";
        internal const string SIGNATURE_HEADER_SHA512_KEY = "sha512";
        #endregion

        #region Properties
        /// <summary>
        /// Gets the receiver name for this receiver.
        /// </summary>
        public static string ReceiverName => RECEIVER_NAME;

        /// <summary>
        /// Gets the case-insensitive name of the WebHook generator that this receiver supports, for example <c>dropbox</c> or <c>net</c>.
        ///  The name provided here will map to a URI of the form '<c>https://&lt;host&gt;/api/webhooks/incoming/&lt;name&gt;</c>'.
        /// </summary>
        public override string Name => RECEIVER_NAME;
        #endregion

        #region Methods
        /// <summary>
        /// Processes the incoming WebHook request. The request may be an initialization request or it may be 
        /// an actual WebHook request. It is up to the receiver to determine what kind of incoming request it
        /// is and process it accordingly.
        /// </summary>
        /// <param name="id">A (potentially empty) ID of a particular configuration for this <see cref="IWebHookReceiver"/>. This
        /// allows an <see cref="IWebHookReceiver"/> to support multiple WebHooks with individual configurations.</param>
        /// <param name="context">The <see cref="HttpRequestContext"/> for the incoming request.</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> containing the incoming WebHook.</param>
        public override async Task<HttpResponseMessage> ReceiveAsync(string id, HttpRequestContext context, HttpRequestMessage request)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            WebSubSubscription subscription = await request.GetWebSubSubscriptionsStore().RetrieveAsync(id);
            if (subscription != null)
            {
                if (request.Method == HttpMethod.Get)
                {
                    return await HandleIntentVerificationAsync(subscription, request);
                }
                else if (request.Method == HttpMethod.Post)
                {
                    HttpResponseMessage contentDistributionVerificationResponse = await VerifyContentDistribution(subscription, request);
                    if (contentDistributionVerificationResponse != null)
                    {
                        return contentDistributionVerificationResponse;
                    }

                    return await ExecuteWebHookAsync(id, context, request, Enumerable.Empty<string>(), new WebSubContent(request));
                }
                else
                {
                    return CreateBadMethodResponse(request);
                }
            }
            else
            {
                return request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        private static Task<HttpResponseMessage> HandleIntentVerificationAsync(WebSubSubscription subscription, HttpRequestMessage request)
        {
            Dictionary<string, string> requestQuery = request.GetQueryNameValuePairs().ToDictionary(nameValuePair => nameValuePair.Key, nameValuePair => nameValuePair.Value, StringComparer.OrdinalIgnoreCase);

            if (requestQuery.TryGetValue(MODE_QUERY_PARAMETER_NAME, out string mode))
            {
                switch (mode)
                {
                    case MODE_DENIED:
                        return HandleSubscribeIntentDenyAsync(request, requestQuery, subscription);
                    case MODE_SUBSCRIBE:
                        return HandleSubscribeIntentVerificationAsync(request, requestQuery, subscription);
                    case MODE_UNSUBSCRIBE:
                        return HandleUnsubscribeIntentVerificationAsync(request, requestQuery, subscription);
                    default:
                        return Task.FromResult(HandleBadRequest(request, $"A '{ReceiverName}' WebHook intent verification request contains unknown '{MODE_QUERY_PARAMETER_NAME}' query parameter value."));
                }
            }
            else
            {
                return Task.FromResult(HandleMissingIntentVerificationParameter(request, MODE_QUERY_PARAMETER_NAME));
            }
        }

        private static async Task<HttpResponseMessage> HandleSubscribeIntentDenyAsync(HttpRequestMessage request, Dictionary<string, string> requestQuery, WebSubSubscription subscription)
        {
            if (!requestQuery.ContainsKey(TOPIC_QUERY_PARAMETER_NAME))
            {
                return HandleBadRequest(request, $"A '{ReceiverName}' WebHook subscribe intent deny request must contain a '{TOPIC_QUERY_PARAMETER_NAME}' query parameter.");
            }
            requestQuery.TryGetValue(INTENT_DENY_REASON_QUERY_PARAMETER_NAME, out string reason);

            subscription.State = WebSubSubscriptionState.SubscribeDenied;

            IWebSubSubscriptionsStore subscriptionsStore = request.GetWebSubSubscriptionsStore();
            await subscriptionsStore.UpdateAsync(subscription);

            IWebSubSubscriptionsService subscriptionsService = request.GetWebSubSubscriptionsService();
            if (subscriptionsService != null)
            {
                await subscriptionsService.OnSubscribeIntentDenyAsync(subscription, reason, subscriptionsStore);
            }

            request.GetWebHooksLogger().Info($"Received a subscribe intent deny request for the '{ReceiverName}' WebHook receiver -- subscription denied, returning confirmation response.");

            return request.CreateResponse(HttpStatusCode.NoContent);
        }

        private static async Task<HttpResponseMessage> HandleSubscribeIntentVerificationAsync(HttpRequestMessage request, Dictionary<string, string> requestQuery, WebSubSubscription subscription)
        {
            if (!requestQuery.TryGetValue(TOPIC_QUERY_PARAMETER_NAME, out string topic) || String.IsNullOrEmpty(topic))
            {
                return HandleMissingIntentVerificationParameter(request, TOPIC_QUERY_PARAMETER_NAME);
            }

            if (!requestQuery.TryGetValue(INTENT_VERIFICATION_CHALLENGE_QUERY_PARAMETER_NAME, out string challenge) || String.IsNullOrEmpty(challenge))
            {
                return HandleMissingIntentVerificationParameter(request, INTENT_VERIFICATION_CHALLENGE_QUERY_PARAMETER_NAME);
            }

            if (!requestQuery.TryGetValue(INTENT_VERIFICATION_LEASE_SECONDS_QUERY_PARAMETER_NAME, out string leaseSecondsValue) || String.IsNullOrEmpty(leaseSecondsValue) || !Int32.TryParse(leaseSecondsValue, out int leaseSeconds))
            {
                return HandleMissingIntentVerificationParameter(request, INTENT_VERIFICATION_LEASE_SECONDS_QUERY_PARAMETER_NAME);
            }

            if (await VerifySubscribeIntentAsync(request, subscription, topic, leaseSeconds))
            {
                request.GetWebHooksLogger().Info($"Received a subscribe intent verification request for the '{ReceiverName}' WebHook receiver -- verification passed, returning challenge response.");
                return CreateChallengeResponse(challenge);
            }
            else
            {
                request.GetWebHooksLogger().Info($"Received a subscribe intent verification request for the '{ReceiverName}' WebHook receiver -- verification failed, returning challenge response.");
                return request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        private static async Task<bool> VerifySubscribeIntentAsync(HttpRequestMessage request, WebSubSubscription subscription, string topic, int leaseSeconds)
        {
            bool verified = false;

            if (subscription.State == WebSubSubscriptionState.SubscribeRequested)
            {
                IWebSubSubscriptionsStore subscriptionsStore = request.GetWebSubSubscriptionsStore();
                IWebSubSubscriptionsService subscriptionsService = request.GetWebSubSubscriptionsService();

                if (subscription.TopicUrl != topic)
                {
                    if (subscriptionsService != null)
                    {
                        await subscriptionsService.OnInvalidSubscribeIntentVerificationAsync(subscription, subscriptionsStore);
                    }
                }
                else if ((subscriptionsService == null) || (await subscriptionsService.OnSubscribeIntentVerificationAsync(subscription, subscriptionsStore)))
                {
                    subscription.State = WebSubSubscriptionState.SubscribeValidated;
                    subscription.VerificationRequestTimeStampUtc = DateTime.UtcNow;
                    subscription.LeaseSeconds = leaseSeconds;
                    await subscriptionsStore.UpdateAsync(subscription);

                    verified = true;
                }
            }

            return verified;
        }

        private static async Task<HttpResponseMessage> HandleUnsubscribeIntentVerificationAsync(HttpRequestMessage request, Dictionary<string, string> requestQuery, WebSubSubscription subscription)
        {
            if (!requestQuery.TryGetValue(TOPIC_QUERY_PARAMETER_NAME, out string topic) || String.IsNullOrEmpty(topic))
            {
                return HandleMissingIntentVerificationParameter(request, TOPIC_QUERY_PARAMETER_NAME);
            }

            if (!requestQuery.TryGetValue(INTENT_VERIFICATION_CHALLENGE_QUERY_PARAMETER_NAME, out string challenge) || String.IsNullOrEmpty(challenge))
            {
                return HandleMissingIntentVerificationParameter(request, INTENT_VERIFICATION_CHALLENGE_QUERY_PARAMETER_NAME);
            }

            if (await VerifyUnsubscribeIntentAsync(request, subscription, topic))
            {
                request.GetWebHooksLogger().Info($"Received an unsubscribe intent verification request for the '{ReceiverName}' WebHook receiver -- verification passed, returning challenge response.");
                return CreateChallengeResponse(challenge);
            }
            else
            {
                request.GetWebHooksLogger().Info($"Received an unsubscribe intent verification request for the '{ReceiverName}' WebHook receiver -- verification failed, returning challenge response.");
                return request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        private static async Task<bool> VerifyUnsubscribeIntentAsync(HttpRequestMessage request, WebSubSubscription subscription, string topic)
        {
            bool verified = false;

            if (subscription.State == WebSubSubscriptionState.UnsubscribeRequested)
            {
                IWebSubSubscriptionsStore subscriptionsStore = request.GetWebSubSubscriptionsStore();
                IWebSubSubscriptionsService subscriptionsService = request.GetWebSubSubscriptionsService();

                if (subscription.TopicUrl != topic)
                {
                    if (subscriptionsService != null)
                    {
                        await subscriptionsService.OnInvalidUnsubscribeIntentVerificationAsync(subscription, subscriptionsStore);
                    }
                }
                else if ((subscriptionsService == null) || (await subscriptionsService.OnUnsubscribeIntentVerificationAsync(subscription, subscriptionsStore)))
                {
                    subscription.State = WebSubSubscriptionState.UnsubscribeValidated;
                    subscription.VerificationRequestTimeStampUtc = DateTime.UtcNow;
                    await subscriptionsStore.UpdateAsync(subscription);

                    verified = true;
                }
            }

            return verified;
        }

        private static HttpResponseMessage HandleMissingIntentVerificationParameter(HttpRequestMessage request, string parameterName)
        {
            return HandleBadRequest(request, $"A '{ReceiverName}' WebHook intent verification request must contain a '{parameterName}' query parameter.");
        }

        private static HttpResponseMessage CreateChallengeResponse(string challenge)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(challenge, System.Text.Encoding.UTF8, "text/plain")
            };
        }

        private async Task<HttpResponseMessage> VerifyContentDistribution(WebSubSubscription subscription, HttpRequestMessage request)
        {
            if (subscription.State != WebSubSubscriptionState.SubscribeValidated)
            {
                return request.CreateResponse(HttpStatusCode.NotFound);
            }

            if (String.IsNullOrWhiteSpace(subscription.Secret))
            {
                return null;
            }

            string signatureHeader;

            try
            {
                signatureHeader = GetRequestHeader(request, SIGNATURE_HEADER_NAME);
            }
            catch (HttpResponseException)
            {
                return HandleInvalidSignatureHeader(request);
            }

            string[] tokens = signatureHeader.SplitAndTrim('=');
            if (tokens.Length != 2)
            {
                return HandleInvalidSignatureHeader(request);
            }

            byte[] signatureHeaderExpectedHash;
            try
            {
                signatureHeaderExpectedHash = EncodingUtilities.FromHex(tokens[1]);

            }
            catch (Exception)
            {
                return HandleBadRequest(request, $"The '{SIGNATURE_HEADER_NAME}' header value is invalid. The '{RECEIVER_NAME}' WebHook receiver requires a valid hex-encoded string.");
            }

            byte[] payloadActualHash = await ComputeRequestBodyHashAsync(request, tokens[0], Encoding.UTF8.GetBytes(subscription.Secret));
            if (payloadActualHash == null)
            {
                return HandleInvalidSignatureHeader(request);
            }

            if (!SecretEqual(signatureHeaderExpectedHash, payloadActualHash))
            {
                return HandleBadRequest(request, $"The signature provided by the '{SIGNATURE_HEADER_NAME}' header field does not match the value expected by the '{RECEIVER_NAME}' WebHook receiver. WebHook request is invalid.");
            }

            return null;
        }

        private static async Task<byte[]> ComputeRequestBodyHashAsync(HttpRequestMessage request, string signatureHeaderKey, byte[] secret)
        {
            if (String.Equals(signatureHeaderKey, SIGNATURE_HEADER_SHA1_KEY, StringComparison.OrdinalIgnoreCase))
            {
                using (HMACSHA1 hasher = new HMACSHA1(secret))
                {
                    return await ComputeRequestBodyHmacHashAsync(request, hasher);
                }
            }

            if (String.Equals(signatureHeaderKey, SIGNATURE_HEADER_SHA256_KEY, StringComparison.OrdinalIgnoreCase))
            {
                using (HMACSHA256 hasher = new HMACSHA256(secret))
                {
                    return await ComputeRequestBodyHmacHashAsync(request, hasher);
                }
            }

            if (String.Equals(signatureHeaderKey, SIGNATURE_HEADER_SHA384_KEY, StringComparison.OrdinalIgnoreCase))
            {
                using (HMACSHA384 hasher = new HMACSHA384(secret))
                {
                    return await ComputeRequestBodyHmacHashAsync(request, hasher);
                }
            }

            if (String.Equals(signatureHeaderKey, SIGNATURE_HEADER_SHA512_KEY, StringComparison.OrdinalIgnoreCase))
            {
                using (HMACSHA512 hasher = new HMACSHA512(secret))
                {
                    return await ComputeRequestBodyHmacHashAsync(request, hasher);
                }
            }

            return null;
        }

        private static async Task<byte[]> ComputeRequestBodyHmacHashAsync(HttpRequestMessage request, HMAC hasher)
        {
            byte[] requestBody = await request.Content.ReadAsByteArrayAsync();

            return hasher.ComputeHash(requestBody);
        }

        private static HttpResponseMessage HandleInvalidSignatureHeader(HttpRequestMessage request)
        {
            return HandleBadRequest(request, $"Invalid '{SIGNATURE_HEADER_NAME}' header value. Expecting a value of '{SIGNATURE_HEADER_SHA1_KEY}|{SIGNATURE_HEADER_SHA256_KEY}|{SIGNATURE_HEADER_SHA384_KEY}|{SIGNATURE_HEADER_SHA512_KEY}=<value>'.");
        }

        private static HttpResponseMessage HandleBadRequest(HttpRequestMessage request, string message)
        {
            request.GetWebHooksLogger().Warn(message);

            return request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
        }
        #endregion
    }
}
