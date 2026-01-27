using Microsoft.AspNetCore.Mvc;
using OnePro.Front.Models;
using OnePro.Front.Services.Interfaces;

namespace OnePro.Front.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        public IActionResult Forbidden()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.LoginAsync(model);

            if (!result.success)
            {
                ViewBag.Error = result.message;
                return View(model);
            }

            HttpContext.Session.SetString("JwtToken", result.token);
            HttpContext.Session.SetString("UserName", result.user.Name);
            HttpContext.Session.SetString("UserEmail", result.user.Email);
            HttpContext.Session.SetString("UserRole", result.user.Role.ToString());

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.RegisterAsync(model);

            if (!result.success)
            {
                ViewBag.Error = result.message;
                return View(model);
            }

            HttpContext.Session.SetString("JwtToken", result.token);

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            foreach (var cookieKey in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookieKey);
            }

            return RedirectToAction(nameof(Login));
        }
    }
}
