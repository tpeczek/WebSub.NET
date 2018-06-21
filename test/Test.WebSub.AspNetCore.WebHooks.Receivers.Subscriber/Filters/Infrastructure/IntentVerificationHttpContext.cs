using System;
using System.Threading;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Authentication;
using WebSub.AspNetCore.Services;

namespace Test.WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters.Infrastructure
{
    internal class IntentVerificationHttpContext : WebSubHttpContext
    {
        private const string HTTP_CONTEXT_ITEMS_SUBSCRIPTION_KEY = "WebSub.AspNetCore.WebHooks.Receivers.Subscriber-" + nameof(WebSubSubscription);

        public IntentVerificationHttpContext(string mode, string topic, string challenge, string leaseSeconds, string reason, WebSubSubscription subscription = null, IServiceProvider requestServices = null)
            : base(new IntentVerificationHttpRequest(mode, topic, challenge, leaseSeconds, reason), requestServices)
        {
            Items[HTTP_CONTEXT_ITEMS_SUBSCRIPTION_KEY] = subscription;
        }

        public override void Abort()
        {
            throw new NotImplementedException();
        }
    }
}
