using System;
using System.IO;
using System.Threading;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;

namespace Test.WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters.Infrastructure
{
    internal class WebSubHttpContext : HttpContext
    {
        private IDictionary<object, object> _items = new Dictionary<object, object>();

        public override IFeatureCollection Features => throw new NotImplementedException();

        public override HttpRequest Request { get; }

        public override HttpResponse Response => throw new NotImplementedException();

        public override ConnectionInfo Connection => throw new NotImplementedException();

        public override WebSocketManager WebSockets => throw new NotImplementedException();

        public override AuthenticationManager Authentication => throw new NotImplementedException();

        public override ClaimsPrincipal User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override IDictionary<object, object> Items { get { return _items; } set { _items = value; } }

        public override IServiceProvider RequestServices { get; set; }

        public override CancellationToken RequestAborted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected WebSubHttpContext(WebSubHttpRequest request, IServiceProvider requestServices = null)
        {
            Request = request;
            RequestServices = requestServices;
        }

        public WebSubHttpContext(string method, Stream body = null, IServiceProvider requestServices = null)
            : this(new WebSubHttpRequest(method, body), requestServices)
        { }

        public override void Abort()
        {
            throw new NotImplementedException();
        }
    }
}
