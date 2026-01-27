using OnePro.Front.Models;
using Newtonsoft.Json;
using OnePro.Front.Services.Interfaces;
using RestSharp;

namespace OnePro.Front.Services.Implement
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;

        public AuthService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<(bool success, string message, string token, UserResponse user)> LoginAsync(LoginRequest model)
        {
            string apiUrl = $"{_config["ApiUrl"]}/api/Auth/login";

            var client = new RestClient(apiUrl);
            var request = new RestRequest("", Method.Post);

            request.AddJsonBody(model);

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                return (false, response.Content ?? "Login failed", "", null);

            // Parse JSON
            var result = JsonConvert.DeserializeObject<AuthResponse>(response.Content!);

            return (true, result!.Message, result.Token, result.User);
        }

        public async Task<(bool success, string message, string token, UserResponse user)> RegisterAsync(RegisterRequest model)
        {
            string apiUrl = $"{_config["ApiUrl"]}/api/Auth/register";

            var client = new RestClient(apiUrl);
            var request = new RestRequest("", Method.Post);

            request.AddJsonBody(model);

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                return (false, response.Content ?? "Register failed", "", null);

            var result = JsonConvert.DeserializeObject<AuthResponse>(response.Content!);

            return (true, result!.Message, result.Token, result.User);
        }
    }
}
