namespace Core.ViewModels
{
    public class GroupResponse
    {
        public Guid Id { get; set; }
        public string NamaDivisi { get; set; } = default!;
        public string NamaPerusahaan { get; set; } = default!;
    }

    public class InviteRequest
    {
        public Guid GroupId { get; set; }
        public string Email { get; set; } = default!;
        public int Role { get; set; }
    }

    public class UpdateRoleRequest
    {
        public Guid MemberId { get; set; }
        public int Role { get; set; }
    }

    public class DeleteMemberRequest
    {
        public Guid MemberId { get; set; }
    }
}
