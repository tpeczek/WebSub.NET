using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Test.WebSub.AspNetCore.WebHooks.Receivers.Subscriber.Filters.Infrastructure
{
    internal class WebSubHttpRequest : HttpRequest
    {
        #region Classes
        private class WebSubHeaderDictionary : Dictionary<string, StringValues>, IHeaderDictionary
        {
            public long? ContentLength { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }
        #endregion

        #region Properties
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

        public override IHeaderDictionary Headers { get; }

        public override IRequestCookieCollection Cookies { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override long? ContentLength { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override string ContentType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override Stream Body { get; set; }

        public override bool HasFormContentType => throw new NotImplementedException();

        public override IFormCollection Form { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion

        #region Constructor
        public WebSubHttpRequest(string method, Stream body = null)
        {
            Method = method;
            Scheme = "https";
            IsHttps = true;
            Headers = new WebSubHeaderDictionary();
            Body = body ?? Stream.Null;
        }
        #endregion

        #region Methods
        public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
