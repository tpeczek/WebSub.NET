using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebSub.AspNetCore.Services;
using StoreableWebSubSubscription = WebSub.AspNetCore.Services.WebSubSubscription;
using WebSub.Net.Http.Subscriber;
using WebSub.Net.Http.Subscriber.Discovery;
using Demo.AspNetCore.WebSub.Subscriber.Model;

namespace Demo.AspNetCore.WebSub.Subscriber.Controllers
{
    public class SubscriptionsController : Controller
    {
        #region Fields
        private readonly IWebSubSubscriptionsStore _webSubSubscriptionsStore;
        #endregion

        #region Constructor
        public SubscriptionsController(IWebSubSubscriptionsStore webSubSubscriptionsStore)
        {
            _webSubSubscriptionsStore = webSubSubscriptionsStore;
        }
        #endregion

        #region Actions
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

            StoreableWebSubSubscription webSubSubscription = null;
            try
            {
                webSubSubscription = await _webSubSubscriptionsStore.CreateAsync();

                webSubSubscription.HubUrl = (await webSubSubscriber.SubscribeAsync(
                    new WebSubSubscribeParameters(subscribeViewModel.Url, webSubSubscription.CallbackUrl)
                    {
                        OnDiscoveredAsync = async (WebSubDiscovery discovery, CancellationToken cancellationToken) =>
                        {
                            webSubSubscription.State = WebSubSubscriptionState.SubscribeRequested;
                            webSubSubscription.TopicUrl = discovery.TopicUrl;

                            await _webSubSubscriptionsStore.UpdateAsync(webSubSubscription);
                        }
                    },
                    HttpContext.RequestAborted)
                ).HubUrl;

                return new SubscriptionViewModel(webSubSubscription);
            }
            catch (Exception ex) when ((ex is WebSubDiscoveryException) || (ex is WebSubSubscriptionException))
            {
                await _webSubSubscriptionsStore.RemoveAsync(webSubSubscription);

                return new SubscriptionViewModel(ex);
            }
        }
        #endregion
    }
}
