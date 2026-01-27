namespace OnePro.Front.Models
{
    public class GroupCreateResponse
    {
        public string Message { get; set; } = "";
        public string Token { get; set; } = "";
        public GroupResponse Group { get; set; } = new();
        public UserResponse User { get; set; } = new();
    }
}
