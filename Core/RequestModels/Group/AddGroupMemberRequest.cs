using Core.Models.Enums;

namespace Core.RequestModels.Group
{
    public class AddGroupMemberRequest
    {
        public string Email { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Position { get; set; } = default!;
        public Role Role { get; set; }
        public string? Password { get; set; }
    }
}
