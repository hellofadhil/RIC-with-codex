using Microsoft.AspNetCore.Http;

namespace Core.Helpers
{
    public static class HttpContextHelper
    {
        private static IHttpContextAccessor _contextAccessor;

        public static void Configure(IHttpContextAccessor accessor)
        {
            _contextAccessor = accessor;
        }

        public static string? GetToken()
        {
            return _contextAccessor?.HttpContext?.Session.GetString("JwtToken");
        }
    }
}
