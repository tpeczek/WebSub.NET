using Microsoft.AspNetCore.Mvc;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber;

namespace Demo.AspNetCore.WebSub.Subscriber.Controllers
{
    public class WebSubController : ControllerBase
    {
        // "/api/webhooks/incoming/websub/{id}"
        [WebSubWebHook]
        public IActionResult HandlerForContentDistribution(string id)
        {
            return Ok();
        }
    }
}
