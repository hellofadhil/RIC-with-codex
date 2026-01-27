// using Core.Settings;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.Extensions.DependencyInjection;

// namespace Core.Extensions;

// public static class AuthExtensions
// {
//     public static IServiceCollection AddAuth(
//         this IServiceCollection services,
//         JwtSettings jwtSettings
//     )
//     {
//         services
//             .AddAuthentication(options =>
//             {
//                 options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//                 options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//                 options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//             })
//             .AddJwtBearer(options =>
//             {
//                 options.Authority = jwtSettings.Authority;
//                 // options.Audience = jwtSettings.Audience;

//                 options.Audience = "idproo.api";
//                 options.RequireHttpsMetadata = false;
//             });

//         return services;
//     }

//     public static IApplicationBuilder UseAuth(this IApplicationBuilder app)
//     {
//         app.UseAuthentication();

//         app.UseAuthorization();

//         return app;
//     }
// }
