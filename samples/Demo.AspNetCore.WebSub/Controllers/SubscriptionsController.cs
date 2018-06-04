using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebSub.Net.Http.Subscriber;
using WebSub.Net.Http.Subscriber.Discovery;
using Demo.AspNetCore.WebSub.Model;

namespace Demo.AspNetCore.WebSub.Controllers
{
    public class SubscriptionsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<SubscriptionViewModel> Subscribe(SubscribeViewModel subscribeViewModel, WebSubSubscriber webSubSubscriber)
        {
            if (!ModelState.IsValid)
            {
                return new SubscriptionViewModel(ModelState);
            }

            try
            {
                string subscriptionId = Guid.NewGuid().ToString("D");
                string callbackUrl = $"https://demo.aspnetcore.websub/api/webhooks/incoming/websub/{subscriptionId}";

                WebSubSubscription webSubSubscription = await webSubSubscriber.SubscribeAsync(new WebSubSubscribeParameters(subscribeViewModel.Url, callbackUrl), HttpContext.RequestAborted);

                return new SubscriptionViewModel(webSubSubscription);
            }
            catch (Exception ex) when ((ex is WebSubDiscoveryException) || (ex is WebSubSubscriptionException))
            {
                return new SubscriptionViewModel(ex);
            }
        }
    }
}
