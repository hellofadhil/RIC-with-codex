namespace Core.ViewModels
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public Guid IdGroup { get; set; }

        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;

        public int Role { get; set; }
        public string Position { get; set; } = default!;
        public string? TandaTanganFile { get; set; }
    }
}
