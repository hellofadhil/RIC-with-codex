namespace Core.ViewModels
{
    public class FormRicHistoryResponse
    {
        public Guid Id { get; set; }

        public Guid IdFormRic { get; set; }
        public Guid IdEditor { get; set; }

        public int Version { get; set; }
        public string Snapshot { get; set; } = default!;
        public string? EditedFields { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
