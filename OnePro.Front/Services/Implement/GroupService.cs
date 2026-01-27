using Newtonsoft.Json;
using OnePro.Front.Models;
using OnePro.Front.Services.Interfaces;
using RestSharp;

namespace OnePro.Front.Services.Implement
{
    public class GroupService : IGroupService
    {
        private readonly IConfiguration _config;

        public GroupService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<GroupResponse?> GetMyGroupAsync(string token)
        {
            string apiUrl = $"{_config["ApiUrl"]}/api/Group/my";

            var client = new RestClient(apiUrl);
            var request = new RestRequest("", Method.Get);

            request.AddHeader("Authorization", $"Bearer {token}");

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                return null;

            var result = JsonConvert.DeserializeObject<GroupResponse>(response.Content!);

            return result;
        }
    }
}
