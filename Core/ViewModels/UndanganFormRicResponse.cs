namespace Core.ViewModels
{
    public class UndanganFormRicResponse
    {
        public Guid Id { get; set; }

        public Guid IdBr { get; set; }
        public Guid IdUser { get; set; }
        public Guid IdGroupUser { get; set; }

        public string EmailUser { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public string Content { get; set; } = default!;
        public string? Link { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
