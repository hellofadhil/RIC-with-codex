namespace Core.Models.Entities
{
    public class FormRicRollOutHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid IdFormRicRollOut { get; set; }
        public Guid IdEditor { get; set; }

        public int Version { get; set; }

        public string SnapshotJson { get; set; } = default!;
        public string? EditedFieldsJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? Editor { get; set; }
        public FormRicRollOut? FormRicRollOut { get; set; }
    }
}
