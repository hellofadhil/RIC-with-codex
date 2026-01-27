using System.Security.Claims;
using Core.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OnePro.API.Auth;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = true,
    Inherited = true
)]
public sealed class RoleRequiredAttribute : Attribute, IAuthorizationFilter
{
    private readonly HashSet<Role> _allowed;

    public RoleRequiredAttribute(params Role[] roles)
    {
        _allowed = roles?.ToHashSet() ?? new HashSet<Role>();
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // ambil role dari claim (support ClaimTypes.Role & custom "role")
        var roleStr = user.FindFirstValue("role") ?? user.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(roleStr))
        {
            context.Result = new ForbidResult();
            return;
        }

        // parse ke enum Role
        if (!Enum.TryParse<Role>(roleStr, ignoreCase: true, out var role))
        {
            context.Result = new ForbidResult();
            return;
        }

        if (_allowed.Count > 0 && !_allowed.Contains(role))
        {
            context.Result = new ForbidResult();
        }
    }
}
