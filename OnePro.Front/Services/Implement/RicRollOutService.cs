using Core.RequestModels.RicRollOut;
using Newtonsoft.Json;
using OnePro.Front.Services.Interfaces;
using RestSharp;

namespace OnePro.Front.Services.Implement
{
    public class RicRollOutService : IRicRollOutService
    {
        private readonly RestClient _client;
        private readonly ILogger<RicRollOutService> _logger;

        private const string BasePath = "/api/v1/RicRollOut";

        public RicRollOutService(IConfiguration config, ILogger<RicRollOutService> logger)
        {
            _logger = logger;

            var baseUrl = config["ApiUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("ApiUrl belum di-set di appsettings.");

            _client = new RestClient(new RestClientOptions(baseUrl));
        }

        public async Task<List<RicRollOutListItemResponse>> GetMyRollOutsAsync(string token)
        {
            var req = CreateRequest($"{BasePath}/my", Method.Get, token);
            var res = await Execute(req);

            if (!res.IsSuccessful || string.IsNullOrWhiteSpace(res.Content))
                return new List<RicRollOutListItemResponse>();

            return JsonConvert.DeserializeObject<List<RicRollOutListItemResponse>>(res.Content)
                ?? new List<RicRollOutListItemResponse>();
        }

        public async Task<FormRicRollOutDetailResponse?> GetDetailByIdAsync(Guid id, string token)
        {
            var req = CreateRequest($"{BasePath}/{id}/detail", Method.Get, token);
            var res = await Execute(req);

            if (!res.IsSuccessful || string.IsNullOrWhiteSpace(res.Content))
                return null;

            return JsonConvert.DeserializeObject<FormRicRollOutDetailResponse>(res.Content);
        }

        public async Task CreateAsync(CreateRicRollOutRequest request, string token)
        {
            var req = CreateRequest($"{BasePath}", Method.Post, token);
            req.AddJsonBody(request);

            var res = await Execute(req);
            EnsureSuccessOrThrow(res, "Create RollOut gagal");
        }

        public async Task UpdateAsync(Guid id, UpdateRicRollOutRequest request, string token)
        {
            var req = CreateRequest($"{BasePath}/{id}", Method.Put, token);
            req.AddJsonBody(request);

            var res = await Execute(req);
            EnsureSuccessOrThrow(res, "Update RollOut gagal");
        }

        public async Task RejectAsync(Guid id, string? note, string token)
        {
            var req = CreateRequest($"{BasePath}/{id}/reject", Method.Put, token);
            req.AddJsonBody(new RejectRicRollOutRequest { Catatan = note ?? "" });

            var res = await Execute(req);
            EnsureSuccessOrThrow(res, "Reject RollOut gagal");
        }

        public async Task ResubmitAsync(Guid id, UpdateRicRollOutRequest request, string token)
        {
            var req = CreateRequest($"{BasePath}/{id}/resubmit", Method.Put, token);
            req.AddJsonBody(request);

            var res = await Execute(req);
            EnsureSuccessOrThrow(res, "Resubmit RollOut gagal");
        }

        public async Task<bool> ForwardAsync(Guid id, UpdateRicRollOutRequest request, string token)
        {
            var req = CreateRequest($"{BasePath}/{id}/forward", Method.Put, token);
            req.AddJsonBody(request);

            var res = await Execute(req);

            _logger.LogInformation(
                "ROLLOUT FORWARD | Id={Id} | Code={Code} | Success={Success} | Body={Body}",
                id,
                (int)res.StatusCode,
                res.IsSuccessful,
                res.Content
            );

            return res.IsSuccessful;
        }

        public async Task<bool> ApproveAsync(Guid id, string token)
        {
            var req = CreateRequest($"{BasePath}/{id}/approve", Method.Put, token);
            req.AddJsonBody(new { });

            var res = await Execute(req);

            _logger.LogInformation(
                "ROLLOUT APPROVE | Id={Id} | Code={Code} | Success={Success} | Body={Body}",
                id,
                (int)res.StatusCode,
                res.IsSuccessful,
                res.Content
            );

            return res.IsSuccessful;
        }

        // ===== Helpers =====

        private RestRequest CreateRequest(string resource, Method method, string token)
        {
            var req = new RestRequest(resource, method);
            req.AddHeader("Authorization", $"Bearer {token}");
            req.AddHeader("Content-Type", "application/json");
            return req;
        }

        private async Task<RestResponse> Execute(RestRequest request)
        {
            var res = await _client.ExecuteAsync(request);

            if (!res.IsSuccessful)
            {
                _logger.LogWarning(
                    "API ERROR | Url={Url} | Code={Code} | Body={Body}",
                    request.Resource,
                    (int)res.StatusCode,
                    res.Content
                );
            }

            return res;
        }

        private static void EnsureSuccessOrThrow(RestResponse res, string message)
        {
            var code = (int)res.StatusCode;
            if (code < 200 || code > 299)
                throw new Exception($"{message}: {res.StatusCode} - {res.Content}");
        }
    }
}
