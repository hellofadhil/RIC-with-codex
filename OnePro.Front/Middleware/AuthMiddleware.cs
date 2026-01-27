// AuthMiddleware
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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

        if (!string.IsNullOrWhiteSpace(token)
            && (path!.StartsWith("/ric", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/ricrollout", StringComparison.OrdinalIgnoreCase)))
        {
            if (!HasGroupId(token))
            {
                context.Response.Redirect("/Group/Index");
                return;
            }
        }

        await _next(context);
    }

    private static bool HasGroupId(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var groupId = jwt.Claims.FirstOrDefault(c => c.Type == "groupId")?.Value;
            return Guid.TryParse(groupId, out var parsed) && parsed != Guid.Empty;
        }
        catch
        {
            return false;
        }
    }
}
