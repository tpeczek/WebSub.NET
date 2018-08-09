using System;
using WebSub.WebHooks.Receivers.Subscriber.Services;

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
