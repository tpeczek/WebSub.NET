using System;
using System.Threading;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Authentication;

namespace Test.WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters.Infrastructure
{
    internal class IntentVerificationHttpContext : HttpContext
    {
        public override IFeatureCollection Features => throw new NotImplementedException();

        public override HttpRequest Request { get; }

        public override HttpResponse Response => throw new NotImplementedException();

        public override ConnectionInfo Connection => throw new NotImplementedException();

        public override WebSocketManager WebSockets => throw new NotImplementedException();

        public override AuthenticationManager Authentication => throw new NotImplementedException();

        public override ClaimsPrincipal User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override IDictionary<object, object> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override IServiceProvider RequestServices { get; set; }

        public override CancellationToken RequestAborted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IntentVerificationHttpContext(string mode, string topic, string challenge, string leaseSeconds, IServiceProvider requestServices = null)
        {
            Request = new IntentVerificationHttpRequest(this, mode, topic, challenge, leaseSeconds);
            RequestServices = requestServices;
        }

        public override void Abort()
        {
            throw new NotImplementedException();
        }
    }
}
