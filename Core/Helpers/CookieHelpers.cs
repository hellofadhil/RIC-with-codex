using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.Helpers;

public static class CookiesHelper
{
    public static void Set(IConfiguration configuration, HttpContext context, List<JsonCookie> data, string type = "_onePro", int exp = 30, bool encrypt = false)
    {
        var json = context.Request.Cookies[type];
        var cookies = new JObject();
        if (!string.IsNullOrEmpty(json))
        {
            cookies = JObject.Parse(json);
        }
        foreach (var d in data)
        {
            cookies[d.Key] = (encrypt) ? StringHelpers.Encrypt(d.Val) : d.Val;
        }
        var option = new CookieOptions()
        {
            Secure = true,
            SameSite = SameSiteMode.Strict
        };
        if (!string.IsNullOrEmpty(configuration["Cookie:Path"]))
        {
            option.Path = configuration["Cookie:Path"];
        }
        if (exp > 0)
        {
            option.Expires = DateTime.UtcNow.Add(TimeSpan.FromDays(exp));
        }
        context.Response.Cookies.Append(type, JsonConvert.SerializeObject(cookies, Formatting.Indented), option);
    }

    public static string? Get(HttpContext context, string key, string type = "_onePro", bool encrypt = false)
    {
        var json = context.Request.Cookies[type];
        var cookies = new JObject();
        if (!string.IsNullOrEmpty(json))
        {
            cookies = JObject.Parse(json);
        }
        if (cookies[key] != null)
        {
            return encrypt ? StringHelpers.Decrypt(cookies[key]!.ToString()) : cookies[key]!.ToString();
        }
        return string.Empty;
    }
    public static void Remove(HttpContext context, string key, string type = "_onePro")
    {
        var json = context.Request.Cookies[type];
        var cookies = new JObject();
        if (!string.IsNullOrEmpty(json))
        {
            cookies = JObject.Parse(json);
        }
        if (cookies[key] != null)
        {
            cookies.Property(key)!.Remove();
            CookieOptions option = new()
            {
                Expires = DateTime.UtcNow.AddDays(-1)
            };
            context.Response.Cookies.Append(type, JsonConvert.SerializeObject(cookies, Formatting.Indented), option);
        }
    }
}

public class JsonCookie
{
    public string Key { get; set; } = string.Empty;
    public string Val { get; set; } = string.Empty;
}
