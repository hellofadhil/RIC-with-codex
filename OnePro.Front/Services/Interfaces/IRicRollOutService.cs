using Core.RequestModels.RicRollOut;

namespace OnePro.Front.Services.Interfaces
{
    public interface IRicRollOutService
    {
        Task<List<RicRollOutListItemResponse>> GetMyRollOutsAsync(string token);
        Task<FormRicRollOutDetailResponse?> GetDetailByIdAsync(Guid id, string token);

        Task CreateAsync(CreateRicRollOutRequest request, string token);
        Task UpdateAsync(Guid id, UpdateRicRollOutRequest request, string token);

        Task RejectAsync(Guid id, string? note, string token);

        // sementara: forward/resubmit pakai UpdateRicRollOutRequest biar 1 model aja
        Task ResubmitAsync(Guid id, UpdateRicRollOutRequest request, string token);
        Task<bool> ForwardAsync(Guid id, UpdateRicRollOutRequest request, string token);

        Task<bool> ApproveAsync(Guid id, string token);
    }
}
