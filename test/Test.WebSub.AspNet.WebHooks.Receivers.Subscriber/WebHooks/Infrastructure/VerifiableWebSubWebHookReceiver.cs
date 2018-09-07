using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using WebSub.AspNet.WebHooks.Receivers.Subscriber.WebHooks;

namespace Test.WebSub.AspNet.WebHooks.Receivers.Subscriber.WebHooks.Infrastructure
{
    internal class VerifiableWebSubWebHookReceiver : WebSubWebHookReceiver
    {
        private readonly Func<string, HttpRequestContext, HttpRequestMessage, IEnumerable<string>, object, Task<HttpResponseMessage>> _executeWebHookAsyncFunc;

        public VerifiableWebSubWebHookReceiver(Func<string, HttpRequestContext, HttpRequestMessage, IEnumerable<string>, object, Task<HttpResponseMessage>> executeWebHookAsyncFunc)
        {
            _executeWebHookAsyncFunc = executeWebHookAsyncFunc;
        }

        protected override Task<HttpResponseMessage> ExecuteWebHookAsync(string id, HttpRequestContext context, HttpRequestMessage request, IEnumerable<string> actions, object data)
        {
            return _executeWebHookAsyncFunc(id, context, request, actions, data);
        }
    }
}
