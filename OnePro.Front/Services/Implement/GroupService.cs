using Core.RequestModels.Group;
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

        public async Task<GroupCreateResponse?> CreateGroupAsync(string token, CreateGroupRequest request)
        {
            string apiUrl = $"{_config["ApiUrl"]}/api/Group";

            var client = new RestClient(apiUrl);
            var httpRequest = new RestRequest("", Method.Post);
            httpRequest.AddHeader("Authorization", $"Bearer {token}");
            httpRequest.AddJsonBody(request);

            var response = await client.ExecuteAsync(httpRequest);

            if (!response.IsSuccessful)
                return null;

            return JsonConvert.DeserializeObject<GroupCreateResponse>(response.Content!);
        }

        public async Task<bool> AddMemberAsync(string token, AddGroupMemberRequest request)
        {
            string apiUrl = $"{_config["ApiUrl"]}/api/Group/members";

            var client = new RestClient(apiUrl);
            var httpRequest = new RestRequest("", Method.Post);
            httpRequest.AddHeader("Authorization", $"Bearer {token}");
            httpRequest.AddJsonBody(request);

            var response = await client.ExecuteAsync(httpRequest);
            return response.IsSuccessful;
        }

        public async Task<bool> UpdateRoleAsync(string token, Guid memberId, int role)
        {
            string apiUrl = $"{_config["ApiUrl"]}/api/Group/members/{memberId}/role";

            var client = new RestClient(apiUrl);
            var httpRequest = new RestRequest("", Method.Put);
            httpRequest.AddHeader("Authorization", $"Bearer {token}");
            httpRequest.AddJsonBody(new { role });

            var response = await client.ExecuteAsync(httpRequest);
            return response.IsSuccessful;
        }

        public async Task<bool> DeleteMemberAsync(string token, Guid memberId)
        {
            string apiUrl = $"{_config["ApiUrl"]}/api/Group/members/{memberId}";

            var client = new RestClient(apiUrl);
            var httpRequest = new RestRequest("", Method.Delete);
            httpRequest.AddHeader("Authorization", $"Bearer {token}");

            var response = await client.ExecuteAsync(httpRequest);
            return response.IsSuccessful;
        }
    }
}
