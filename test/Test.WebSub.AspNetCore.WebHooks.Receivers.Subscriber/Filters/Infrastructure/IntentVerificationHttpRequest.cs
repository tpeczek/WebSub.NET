using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace Test.WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters.Infrastructure
{
    internal class IntentVerificationHttpRequest : WebSubHttpRequest
    {
        private const string INTENT_VERIFICATION_MODE_QUERY_PARAMETER_NAME = "hub.mode";
        private const string INTENT_VERIFICATION_TOPIC_QUERY_PARAMETER_NAME = "hub.topic";
        private const string INTENT_VERIFICATION_CHALLENGE_QUERY_PARAMETER_NAME = "hub.challenge";
        private const string INTENT_VERIFICATION_LEASE_SECONDS_QUERY_PARAMETER_NAME = "hub.lease_seconds";
        private const string INTENT_VERIFICATION_REASON_QUERY_PARAMETER_NAME = "hub.reason";

        public IntentVerificationHttpRequest(string mode, string topic, string challenge, string leaseSeconds, string reason)
            : base(HttpMethods.Get)
        {
            Dictionary<String, StringValues> queryValues = new Dictionary<string, StringValues>();

            if (!String.IsNullOrWhiteSpace(mode))
            {
                queryValues.Add(INTENT_VERIFICATION_MODE_QUERY_PARAMETER_NAME, mode);
            }

            if (!String.IsNullOrWhiteSpace(topic))
            {
                queryValues.Add(INTENT_VERIFICATION_TOPIC_QUERY_PARAMETER_NAME, topic);
            }

            if (!String.IsNullOrWhiteSpace(challenge))
            {
                queryValues.Add(INTENT_VERIFICATION_CHALLENGE_QUERY_PARAMETER_NAME, challenge);
            }

            if (!String.IsNullOrWhiteSpace(leaseSeconds))
            {
                queryValues.Add(INTENT_VERIFICATION_LEASE_SECONDS_QUERY_PARAMETER_NAME, leaseSeconds);
            }

            if (!String.IsNullOrWhiteSpace(reason))
            {
                queryValues.Add(INTENT_VERIFICATION_REASON_QUERY_PARAMETER_NAME, reason);
            }

            Query = new QueryCollection(queryValues);
        }
    }
}
