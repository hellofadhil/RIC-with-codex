using Microsoft.AspNetCore.Mvc;

namespace OnePro.Front.Helpers
{
    public static class AuthHelper
    {
        public static bool TryGetAuthToken(
            Controller controller,
            out string? token,
            out IActionResult? redirectResult
        )
        {
            token = controller.HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                redirectResult = controller.RedirectToAction("Login", "Auth");
                return false;
            }

            redirectResult = null;
            return true;
        }
    }
}
