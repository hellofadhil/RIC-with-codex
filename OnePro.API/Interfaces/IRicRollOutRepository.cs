using Core.Models.Entities;
using Core.Models.Enums;
using Core.RequestModels.RicRollOut;

namespace OnePro.API.Interfaces
{
    public interface IRicRollOutRepository
    {
        Task<List<RicRollOutListItemResponse>> GetAllByGroupAsync(Guid groupId);

        Task<FormRicRollOut?> GetByIdAsync(Guid id);
        Task<FormRicRollOutDetailResponse?> GetDetailByIdAsync(Guid id);

        Task<FormRicRollOutDetailResponse?> GetDetailAsync(Guid id);

        Task<bool> CreateAsync(FormRicRollOut model);
        Task<bool> UpdateAsync(FormRicRollOut model);
        Task<bool> DeleteAsync(Guid id);

        Task AddHistoryAsync(FormRicRollOutHistory history);
        Task AddReviewAsync(ReviewFormRicRollOut review);

        Task<bool> MoveRollOutToNextStageAsync(FormRicRollOut model, Guid actorId);
        Task<bool> ResubmitAfterRejectionAsync(FormRicRollOut model, Guid editorId);

        Task<bool> EnsureApprovalsCreatedAsync(Guid rollOutId);
        Task<bool> MarkApprovalApprovedAsync(Guid rollOutId, RoleApproval role, Guid approverId);
    }
}
