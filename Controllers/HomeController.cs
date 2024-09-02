using Microsoft.AspNetCore.Mvc;

namespace DayByDay.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Diary");
            }

            return View();
        }
    }
}