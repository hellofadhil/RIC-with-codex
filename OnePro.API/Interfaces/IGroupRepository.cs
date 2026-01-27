using System;
using System.Threading.Tasks;
using Core.ViewModels;

namespace OnePro.API.Interfaces
{
    public interface IGroupRepository
    {
        Task<GroupWithMembersResponse?> GetGroupWithMembersAsync(Guid groupId);

        // Task<(bool Success, string? Error)> InviteMemberByEmailAsync(Guid groupId, string email, int role);
        // Task<(bool Success, string? Error)> UpdateMemberRoleAsync(Guid memberId, int newRole, Guid performedByUserId);
        // Task<(bool Success, string? Error)> DeleteMemberAsync(Guid memberId, Guid performedByUserId);
        // Task<int?> GetUserRoleInGroupAsync(Guid userId, Guid groupId);

    }
}
