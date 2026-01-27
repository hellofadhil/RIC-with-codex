namespace Core.RequestModels.RicRollOut
{
    public class RicRollOutListItemResponse
    {
        public Guid Id { get; set; }

        public string Entitas { get; set; } = default!;
        public string JudulAplikasi { get; set; } = default!;

        public string? UserName { get; set; }

        public string Status { get; set; } = default!;
        public DateTime UpdatedAt { get; set; }
    }
}
