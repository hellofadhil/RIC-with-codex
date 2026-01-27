using OnePro.Front.Models;
// using Core.RequestModels.RicRollOut;

namespace OnePro.Front.Services.Interfaces
{
    public interface IRicService
    {
        Task<List<RicItemResponse>> GetMyRicsAsync(string token);
        Task<List<RicItemResponse>> GetApprovalQueueAsync(string token);
        Task<RicDetailResponse?> GetRicByIdAsync(Guid id, string token);

        Task CreateRicAsync(FormRicCreateRequest request, string token);
        Task UpdateRicAsync(Guid id, FormRicUpdateRequest request, string token);
        Task<bool> DeleteRicAsync(Guid id, string token);

        // NEW: aksi review (approve/reject)
        // Task ReviewRicAsync(Guid id, RicReviewRequest request, string token);
        Task RejectAsync(Guid id, string? note, string token);
        Task ResubmitRicAsync(Guid id, FormRicResubmitRequest request, string token);
        Task<bool> ForwardAsync(Guid id, FormRicResubmitRequest request, string token);

        Task<bool> ApproveAsync(Guid id, string token);
    }
}
