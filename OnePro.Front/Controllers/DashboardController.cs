using Microsoft.AspNetCore.Mvc;
using OnePro.Front.Middleware;

namespace OnePro.Front.Controllers
{
    // wajib login
    [AuthRequired]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // if (!TryGetToken(out var token))
            //     return RedirectToLogin();
                
            var userRole = HttpContext.Session.GetString("UserRole");
            ViewBag.UserRole = userRole;

            return View("Index");
        }
    }
}
