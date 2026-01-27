using System.ComponentModel.DataAnnotations;

namespace Core.Models.Entities
{
    public class FormRicHistory
    {
        public Guid Id { get; set; }
        public Guid IdFormRic { get; set; }
        public Guid IdEditor { get; set; }

        public int Version { get; set; }

        // snapshot FULL sebelum perubahan
        public string SnapshotJson { get; set; } = default!;

        // diff aja
        public string? EditedFieldsJson { get; set; }

        public DateTime CreatedAt { get; set; }

        public User? Editor { get; set; }
    }
}
