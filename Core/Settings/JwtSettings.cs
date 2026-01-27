// namespace Core.Settings;

// public class JwtSettings
// {
//     public string Issuer { get; set; } = default!;

//     public string Audience { get; set; } = default!;
//     public string Authority { get; set; } = default!;

//     public string Secret { get; set; } = default!;

//     public int ExpirationInDays { get; set; }
// }

namespace Core.Settings
{
    public class JwtSettings
    {
        public string Key { get; set; } = default!;
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public int ExpireMinutes { get; set; }
    }
}
