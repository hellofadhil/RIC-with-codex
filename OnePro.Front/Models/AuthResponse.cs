using Newtonsoft.Json;

namespace OnePro.Front.Models
{
    public class AuthResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("user")]
        public UserResponse User { get; set; }
    }
}
