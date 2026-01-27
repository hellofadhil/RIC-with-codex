namespace Core.RequestModels
{
    public class RegisterRequest
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string Position { get; set; } = default!;
        public Guid IdGroup { get; set; }
    }
}
