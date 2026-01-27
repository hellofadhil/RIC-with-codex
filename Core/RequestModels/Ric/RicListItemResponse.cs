namespace Core.RequestModels.Ric
{
    public class RicListItemResponse
    {
        public Guid Id { get; set; }
        public string Judul { get; set; } = default!;
        public string? Permasalahan { get; set; }
        public string? UserName { get; set; }
        public string Status { get; set; } = default!;
        public DateTime UpdatedAt { get; set; }

        public List<string>? Hastag { get; set; }
    }
}
