using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Lib.AspNetCore.ServerSentEvents;
using WebSub.AspNetCore.Services;
using WebSub.AspNetCore.WebHooks.Receivers.Subscriber;

namespace Demo.AspNetCore.WebSub.Subscriber.Controllers
{
    public class WebSubController : ControllerBase
    {
        #region Fields
        private readonly IServerSentEventsService _serverSentEventsService;
        #endregion

        #region Constructor
        public WebSubController(IServerSentEventsService serverSentEventsService)
        {
            _serverSentEventsService = serverSentEventsService;
        }
        #endregion

        #region Actions
        // "/api/webhooks/incoming/websub/{id}"
        [WebSubWebHook]
        public async Task<IActionResult> HandlerForContentDistribution(string id, WebSubSubscription subscription, IWebSubContent content)
        {
            string contentAsString = await content.ReadAsStringAsync();

            await _serverSentEventsService.SendEventAsync($"HandlerForContentDistribution ({id})");

            return Ok();
        }
        #endregion
    }
}
