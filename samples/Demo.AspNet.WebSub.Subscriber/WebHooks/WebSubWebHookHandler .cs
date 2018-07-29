using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;
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
            object content = context.Data;

            return Task.FromResult(true);
        }
    }

}