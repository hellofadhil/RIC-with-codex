using Core.Models.Entities;
using Core.Models.Enums;
using Core.RequestModels.Ric;

namespace OnePro.API.Interfaces
{
    public interface IRicRepository
    {
        Task<List<RicListItemResponse>> GetAllByGroupAsync(Guid groupId);
        Task<List<RicListItemResponse>> GetApprovalQueueAsync(Guid groupId, string role);
        Task<FormRic?> GetByIdAsync(Guid id);
        Task<FormRicDetailResponse?> GetDetailByIdAsync(Guid id);

        Task<bool> CreateAsync(FormRic model);
        Task<bool> UpdateAsync(FormRic model);
        Task<bool> ResubmitAfterRejection(FormRic model, Guid editorId);
        Task<bool> MoveRicToNextStageAsync(FormRic model, Guid actorId);

        Task<bool> DeleteAsync(Guid id);

        Task AddHistoryAsync(FormRicHistory history);
        Task AddReviewAsync(ReviewFormRic review);
        Task<bool> EnsureApprovalsCreatedAsync(Guid ricId);

        Task<bool> MarkApprovalApprovedAsync(Guid ricId, RoleApproval role, Guid approverId);
    }
}
