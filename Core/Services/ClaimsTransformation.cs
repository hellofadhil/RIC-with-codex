using Core.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators.OAuth2;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Core.Services;

public class ClaimsTransformation(IConfiguration _configuration, IHttpContextAccessor contextAccessor) : IClaimsTransformation
{
    public readonly IConfiguration configuration = _configuration;
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor;

    public string Token()
    {
        string tokenName = "_";
        string token = CookiesHelper.Get(_contextAccessor.HttpContext!, tokenName, "_oneProToken", true)!;
        bool generate = false;
        if (!string.IsNullOrEmpty(token))
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenS = handler.ReadToken(token) as JwtSecurityToken;
            if (DateTimeOffset.Now > tokenS!.Payload.ValidFrom)
            {
                generate = true;
            }
        }
        else
        {
            generate = true;
        }
        if (generate)
        {
            var client = new RestClient($"{configuration["Authentication:IdAMan:UrlLogin"]}/connect/token");
            var request = new RestRequest()
                .AddParameter("client_id", configuration["Authentication:IdAMan:ClientId"]!, ParameterType.GetOrPost)
                .AddParameter("client_secret", configuration["Authentication:IdAMan:ClientSecret"]!, ParameterType.GetOrPost)
                .AddParameter("grant_type", "client_credentials", ParameterType.GetOrPost)
                .AddParameter("scope", configuration["Authentication:IdAMan:Scopes"]!, ParameterType.GetOrPost);
            var response = client.Post(request);
            var obj = JObject.Parse(response.Content!.ToString());
            string newToken = (response != null) ? obj.SelectToken("access_token")?.ToString()! : "";
            int expired = (response != null) ? int.Parse(obj.SelectToken("expires_in")?.ToString()!) : -1;

            CookiesHelper.Set(configuration, _contextAccessor!.HttpContext!, [
                      new JsonCookie()
                      {
                          Key = tokenName,
                          Val = newToken
                      }
                    ], "_oneProToken", expired, true);
            return newToken;
        }
        return token;
    }

    public string TokenInScope(string scopes)
    {
        string tokenName = "_";
        string token = CookiesHelper.Get(_contextAccessor.HttpContext!, tokenName, "_oneProToken", true)!;
        bool generate = false;
        if (!string.IsNullOrEmpty(token))
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenS = handler.ReadToken(token) as JwtSecurityToken;
            if (DateTimeOffset.Now > tokenS!.Payload.ValidFrom)
            {
                generate = true;
            }
        }
        else
        {
            generate = true;
        }
        if (generate)
        {
            var client = new RestClient($"{configuration["Authentication:IdAMan:UrlLogin"]}/connect/token");
            var request = new RestRequest()
                .AddParameter("client_id", configuration["Authentication:IdAMan:ClientId"]!, ParameterType.GetOrPost)
                .AddParameter("client_secret", configuration["Authentication:IdAMan:ClientSecret"]!, ParameterType.GetOrPost)
                .AddParameter("grant_type", "client_credentials", ParameterType.GetOrPost)
                .AddParameter("scope", $"{configuration["Authentication:IdAMan:Scopes"]!} {scopes.Replace(",", " ")}", ParameterType.GetOrPost);
            var response = client.Post(request);
            var obj = JObject.Parse(response.Content!.ToString());
            string newToken = (response != null) ? obj.SelectToken("access_token")?.ToString()! : "";
            int expired = (response != null) ? int.Parse(obj.SelectToken("expires_in")?.ToString()!) : -1;

            CookiesHelper.Set(configuration, _contextAccessor!.HttpContext!, [
                      new JsonCookie()
                      {
                          Key = tokenName,
                          Val = newToken
                      }
                    ], "_appToken", expired, true);
            return newToken;
        }
        return token;
    }

    public JObject UserByEmail(string token, string email)
    {
        var url = $"{configuration["Authentication:IdAMan:UrlApi"]}/users/{email}";
        var options = new RestClientOptions(url)
        {
            Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token, "Bearer")
        };
        var client = new RestClient(options);
        var request = new RestRequest
        {
            Method = Method.Get
        };
        var response = client.Execute(request);
        if (response.IsSuccessful)
        {
            return JObject.Parse(response.Content!);
        }
        return null!;
    }

    public List<string> UserInRoles(string userId)
    {
        var roles = new List<string>();
        string rolesCookies = CookiesHelper.Get(_contextAccessor.HttpContext!, "userInRoles")!;
        if (string.IsNullOrEmpty(rolesCookies) || rolesCookies.Equals("[]"))
        {
            string? token = Token();
            var url = $"{configuration["Authentication:IdAMan:UrlApi"]}/roles/{userId}";
            var options = new RestClientOptions(url)
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token, "Bearer")
            };
            var client = new RestClient(options);
            var request = new RestRequest
            {
                Method = Method.Get
            };
            var response = client.Execute(request);
            if (response.IsSuccessful)
            {
                var data = JArray.Parse(response.Content!);
                // position permanent
                var rolesResponse = JArray.Parse(data[0].SelectToken("roles")?.ToString()!);
                foreach (var role in rolesResponse)
                {
                    if (!roles.Any(x => x.Equals(role.SelectToken("id")?.ToString())))
                    {
                        roles.Add(role.SelectToken("id")?.ToString()!);
                    }
                }
            }
            CookiesHelper.Set(configuration, _contextAccessor.HttpContext!, [
                            new JsonCookie()
                            {
                                Key = "userInRoles",
                                Val = JsonConvert.SerializeObject(roles, Formatting.Indented)
                            }
                        ]);
            return roles;
        }
        var dataRole = JsonConvert.DeserializeObject<List<string>>(CookiesHelper.Get(_contextAccessor.HttpContext!, "userInRoles")!);
        return dataRole!;
    }

    public List<string> Roles()
    {
        var roles = new List<string>();
        string rolesCookies = CookiesHelper.Get(_contextAccessor.HttpContext!, "roles")!;
        if (string.IsNullOrEmpty(rolesCookies) || rolesCookies.Equals("[]"))
        {
            string? token = Token();
            var url = $"{configuration["Authentication:IdAMan:UrlApi"]}/applications/roles/{configuration["Authentication:IdAMan:ObjectId"]}/?page=1&take=1000";
            var options = new RestClientOptions(url)
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token, "Bearer")
            };
            var client = new RestClient(options);
            var request = new RestRequest
            {
                Method = Method.Get
            };
            var response = client.Execute(request);
            if (response.IsSuccessful)
            {
                var data = JObject.Parse(response.Content!);
                var values = JArray.Parse(data.SelectToken("value")?.ToString()!);
                foreach (var value in values)
                {
                    if (!roles.Any(x => x.Equals(value.SelectToken("id")?.ToString())))
                    {
                        roles.Add(value.SelectToken("id")?.ToString()!);
                    }
                }
            }
            CookiesHelper.Set(configuration, _contextAccessor.HttpContext!, [
                        new JsonCookie()
                        {
                            Key = "roles",
                            Val = JsonConvert.SerializeObject(roles, Formatting.Indented)
                        }
                    ]);
            return roles;
        }
        var dataRole = JsonConvert.DeserializeObject<List<string>>(CookiesHelper.Get(_contextAccessor.HttpContext!, "roles")!);
        return dataRole!;
    }


    public List<RoleScope> Access(List<string> roles)
    {
        var result = new List<RoleScope>();
        string? token = Token();
        foreach (var role in roles)
        {
            var url = $"{configuration["Authentication:IdAMan:UrlApi"]}/roles/{role}/users";
            var options = new RestClientOptions(url)
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token, "Bearer")
            };
            var client = new RestClient(options);
            var request = new RestRequest
            {
                Method = Method.Get
            };
            var response = client.Execute(request);
            if (response.IsSuccessful)
            {
                var data = JObject.Parse(response.Content!);
                var value = data.SelectToken("value")?.ToString()!;
                if (!string.IsNullOrEmpty(value))
                {
                    var values = JArray.Parse(value);
                    foreach (var val in values)
                    {
                        result.Add(new RoleScope
                        {
                            UserId = val.SelectToken("id")!.ToString(),
                            Email = val.SelectToken("email")!.ToString(),
                            Scopes = val.SelectToken("scope")!.ToString()
                        });
                    }
                }
            }
        }
        return result;
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {

        var claimsIdentity = new ClaimsIdentity();
        string? token = Token();
        string userCookies = CookiesHelper.Get(_contextAccessor.HttpContext!, "user")!;
        var email = principal?.Claims?.FirstOrDefault(x => x.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"))?.Value;
        var userRoles = new List<string>();
        if (!string.IsNullOrEmpty(email))
        {
            if (!string.IsNullOrEmpty(email))
            {
                var obj = UserByEmail(token, email);
                if (obj != null)
                {
                    userRoles = UserInRoles(obj.SelectToken("id")!.ToString());
                    if (userRoles.Any(x => x.Equals(configuration["Roles:Administrator"])))
                    {
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
                    }
                    else
                    {
                        //if (userRoles.Any(x => x.Equals(configuration["Roles:User"])))
                        //{
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "User"));
                        //}
                    }
                }
            }
        }
        if (string.IsNullOrEmpty(userCookies))
        {
            if (!string.IsNullOrEmpty(email))
            {
                string access_token = "";
                string scopes = "";

                var roles = Roles();
                //foreach (var cl in Access(roles))
                //{
                //    if (cl.Email!.Trim().Equals(email))
                //    {
                //        scopes = cl.Scopes!;
                //        access_token = TokenInScope(cl.Scopes!);
                //        claimsIdentity.AddClaim(new Claim("access_token", access_token));
                //    }
                //}
                scopes = configuration["Jwt:Scope"]!;
                access_token = TokenInScope(configuration["Jwt:Scope"]!);
                claimsIdentity.AddClaim(new Claim("access_token", access_token));

                var data = UserByEmail(token, email);
                claimsIdentity.AddClaim(new Claim("id", data.SelectToken("id")?.ToString()!));
                claimsIdentity.AddClaim(new Claim("email", data.SelectToken("email")?.ToString()!));
                claimsIdentity.AddClaim(new Claim("displayName", data.SelectToken("displayName")?.ToString()!));
                claimsIdentity.AddClaim(new Claim("companyCode", data.SelectToken("companyCode")?.ToString()!));
                claimsIdentity.AddClaim(new Claim("photo", data.SelectToken("photo")?.ToString()!));

                string[] inScopes = scopes.Split(',');
                foreach (var scope in inScopes)
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, scope));
                }

                CookiesHelper.Set(configuration, _contextAccessor.HttpContext!, [
                        new JsonCookie()
                        {
                            Key = "user",
                            Val = JsonConvert.SerializeObject(new JObject(){
                                new JProperty("id", data.SelectToken("id")?.ToString()),
                                new JProperty("email", data.SelectToken("email")?.ToString()),
                                new JProperty("displayName", data.SelectToken("displayName")?.ToString()),
                                new JProperty("companyCode", data.SelectToken("companyCode")?.ToString()),
                                new JProperty("photo",data.SelectToken("photo")?.ToString()),
                                //new JProperty("access_token", access_token),
                                new JProperty("scopes", scopes)
                            }, Formatting.Indented)
                        }
                    ]);
            }
        }
        else
        {
            if (userRoles.Any(x => x.Equals(configuration["Roles:Administrator"])))
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
            }
            else
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "User"));
            }
            var data = JsonConvert.DeserializeObject<JObject>(CookiesHelper.Get(_contextAccessor.HttpContext!, "user")!)!;
            claimsIdentity.AddClaim(new Claim("id", data.SelectToken("id")?.ToString()!));
            claimsIdentity.AddClaim(new Claim("email", data.SelectToken("email")?.ToString()!));
            claimsIdentity.AddClaim(new Claim("displayName", data.SelectToken("displayName")?.ToString()!));
            claimsIdentity.AddClaim(new Claim("companyCode", data.SelectToken("companyCode")?.ToString()!));
            claimsIdentity.AddClaim(new Claim("photo", data.SelectToken("photo")?.ToString()!));
            string inScope = data.SelectToken("scopes")!.ToString();
            claimsIdentity.AddClaim(new Claim("access_token", TokenInScope(inScope)));
            string[] inScopes = inScope.Split(',');
            foreach (var scope in inScopes)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, scope));
            }

        }


        principal!.AddIdentity(claimsIdentity);
        return Task.FromResult(principal);
    }
}
public class RoleScope
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Scopes { get; set; }
}
