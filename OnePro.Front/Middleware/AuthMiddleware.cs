// AuthMiddleware
using Microsoft.AspNetCore.Http;

namespace OnePro.Front.Middleware;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Session.GetString("JwtToken");
        var path = context.Request.Path.Value?.ToLower();

        if (string.IsNullOrWhiteSpace(token) &&
            !path!.StartsWith("/auth", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Redirect("/Auth/Login");
            return;
        }

        await _next(context);
    }
}
