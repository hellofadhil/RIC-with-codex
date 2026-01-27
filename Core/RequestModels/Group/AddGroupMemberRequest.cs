using Core.Models.Enums;

namespace Core.RequestModels.Group
{
    public class AddGroupMemberRequest
    {
        public string Email { get; set; } = default!;
        public Role Role { get; set; }
    }
}
