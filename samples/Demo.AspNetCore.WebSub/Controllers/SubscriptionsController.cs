using Microsoft.AspNetCore.Mvc;

namespace Demo.AspNetCore.WebSub.Controllers
{
    public class SubscriptionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
