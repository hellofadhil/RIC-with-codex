// // RoleRequiredAttribute
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Filters;

// namespace OnePro.Front.Middleware
// {
//     [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
//     public class RoleRequiredAttribute : Attribute, IAuthorizationFilter
//     {
//         private readonly int[] _allowedRoles;

//         public RoleRequiredAttribute() : this(1, 4)
//         {
//         }

//         public RoleRequiredAttribute(params int[] allowedRoles)
//         {
//             _allowedRoles = allowedRoles ?? Array.Empty<int>();
//         }

//         public void OnAuthorization(AuthorizationFilterContext context)
//         {
//             var session = context.HttpContext.Session;

//             var roleValueString = session.GetString("UserRole");
//             if (!int.TryParse(roleValueString, out var roleInt))
//             {
//                 context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
//                 return;
//             }

//             if (!_allowedRoles.Contains(roleInt))
//             {
//                 context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
//             }
//         }
//     }
// }


using System.IdentityModel.Tokens.Jwt;
using Core.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OnePro.Front.Middleware
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RoleRequiredAttribute : Attribute, IAuthorizationFilter
    {
        private readonly HashSet<Role> _allowed;

        public RoleRequiredAttribute(params Role[] allowedRoles)
        {
            _allowed = allowedRoles is { Length: > 0 }
                ? new HashSet<Role>(allowedRoles)
                : new HashSet<Role>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var http = context.HttpContext;

            var token = http.Session.GetString("JwtToken");
            if (string.IsNullOrWhiteSpace(token))
            {
                context.Result = new RedirectResult("/Auth/Login");
                return;
            }

            string? roleStr = null;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                roleStr = jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            }
            catch
            {
                context.Result = new RedirectResult("/Auth/Forbidden");
                return;
            }

            if (string.IsNullOrWhiteSpace(roleStr) || !Enum.TryParse<Role>(roleStr, out var role))
            {
                context.Result = new RedirectResult("/Auth/Forbidden");
                return;
            }

            // kalau attribute dipasang tapi kosong roles-nya, anggap forbid
            if (_allowed.Count == 0 || !_allowed.Contains(role))
            {
                context.Result = new RedirectResult("/Auth/Forbidden");
            }
        }
    }
}
