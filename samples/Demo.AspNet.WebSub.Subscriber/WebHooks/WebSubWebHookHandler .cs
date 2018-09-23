using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;
using WebSub.WebHooks.Receivers.Subscriber;
using WebSub.AspNet.WebHooks.Receivers.Subscriber.WebHooks;

namespace Demo.AspNet.WebSub.Subscriber.WebHooks
{
    public class WebSubWebHookHandler : WebHookHandler
    {
        public WebSubWebHookHandler()
        {
            Receiver = WebSubWebHookReceiver.ReceiverName;
        }

        public override Task ExecuteAsync(string generator, WebHookHandlerContext context)
        {
            IWebSubContent content = context.Data as IWebSubContent;

            return Task.FromResult(true);
        }
    }

}