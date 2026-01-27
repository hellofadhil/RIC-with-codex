using System;
using System.Threading.Tasks;
using Core.ViewModels;

namespace OnePro.API.Interfaces
{
    public interface IGroupRepository
    {
        Task<GroupWithMembersResponse?> GetGroupWithMembersAsync(Guid groupId);
        Task<Core.Models.Entities.Group?> GetGroupByIdAsync(Guid groupId);
        Task<Core.Models.Entities.User?> GetUserByIdAsync(Guid userId);
        Task<Core.Models.Entities.User?> GetUserByEmailAsync(string email);
        Task<bool> CreateGroupAsync(Core.Models.Entities.Group group);
        Task<int> CountMembersAsync(Guid groupId);
        Task<bool> UpdateGroupAsync(Core.Models.Entities.Group group);
        Task<bool> DeleteGroupAsync(Core.Models.Entities.Group group);
        Task<bool> AddMemberAsync(Core.Models.Entities.User user);
        Task<bool> UpdateMemberRoleAsync(Core.Models.Entities.User user, Core.Models.Enums.Role newRole);
        Task<bool> UpdateMemberAsync(Core.Models.Entities.User user);
        Task<bool> DeleteMemberAsync(Core.Models.Entities.User user);

        // Task<(bool Success, string? Error)> InviteMemberByEmailAsync(Guid groupId, string email, int role);
        // Task<(bool Success, string? Error)> UpdateMemberRoleAsync(Guid memberId, int newRole, Guid performedByUserId);
        // Task<(bool Success, string? Error)> DeleteMemberAsync(Guid memberId, Guid performedByUserId);
        // Task<int?> GetUserRoleInGroupAsync(Guid userId, Guid groupId);

    }
}
