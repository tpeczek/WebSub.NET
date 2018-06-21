using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters.Infrastructure
{
    internal class WebSubHttpRequest : HttpRequest
    {
        public override HttpContext HttpContext { get; }

        public override string Method { get; set; }

        public override string Scheme { get; set; }

        public override bool IsHttps { get; set; }

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

        public WebSubHttpRequest(string method)
        {
            Method = method;
            Scheme = "https";
            IsHttps = true;
        }

        public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
