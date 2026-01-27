namespace Core.ViewModels
{
    public class GroupWithMembersResponse
    {
        public Guid Id { get; set; }
        public string NamaDivisi { get; set; } = default!;
        public string NamaPerusahaan { get; set; } = default!;

        public List<Members> Members { get; set; } = new();
    }

    public class Members
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;

        public int Role { get; set; }
        public string Position { get; set; } = default!;
    }
}
