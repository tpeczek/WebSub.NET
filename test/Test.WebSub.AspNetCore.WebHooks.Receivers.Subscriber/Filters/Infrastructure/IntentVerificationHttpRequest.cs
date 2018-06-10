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
    internal class IntentVerificationHttpRequest : HttpRequest
    {
        private const string INTENT_VERIFICATION_MODE_QUERY_PARAMETER_NAME = "hub.mode";
        private const string INTENT_VERIFICATION_TOPIC_QUERY_PARAMETER_NAME = "hub.topic";
        private const string INTENT_VERIFICATION_CHALLENGE_QUERY_PARAMETER_NAME = "hub.challenge";
        private const string INTENT_VERIFICATION_LEASE_SECONDS_QUERY_PARAMETER_NAME = "hub.lease_seconds";

        public override HttpContext HttpContext { get; }

        public override string Method { get; set; }

        public override string Scheme { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override bool IsHttps { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override HostString Host { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override PathString PathBase { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override PathString Path { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override QueryString QueryString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override IQueryCollection Query { get; set; }

        public override string Protocol { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override IHeaderDictionary Headers => throw new NotImplementedException();

        public override IRequestCookieCollection Cookies { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override long? ContentLength { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override string ContentType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override Stream Body { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override bool HasFormContentType => throw new NotImplementedException();

        public override IFormCollection Form { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        internal IntentVerificationHttpRequest(HttpContext httpContext, string mode, string topic, string challenge, string leaseSeconds)
        {
            HttpContext = httpContext;

            Method = HttpMethods.Get;

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

            Query = new QueryCollection(queryValues);
        }

        public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
