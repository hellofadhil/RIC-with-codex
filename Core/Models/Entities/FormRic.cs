using System.ComponentModel.DataAnnotations;
using Core.Models.Enums;
using Newtonsoft.Json;

namespace Core.Models.Entities
{
    public class FormRic
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid IdUser { get; set; }
        public Guid IdGroupUser { get; set; }

        public string Judul { get; set; } = default!;

        public List<string>? Hastag { get; set; }
        public List<string>? AsIsProcessRasciFile { get; set; }

        public string? Permasalahan { get; set; }
        public string? DampakMasalah { get; set; }
        public string? FaktorPenyebabMasalah { get; set; }
        public string? SolusiSaatIni { get; set; }

        public List<string>? AlternatifSolusi { get; set; }
        public List<string>? ToBeProcessBusinessRasciKkiFile { get; set; }
        public string? PotensiValueCreation { get; set; }

        public List<string>? ExcpectedCompletionTargetFile { get; set; }

        public string? HasilSetelahPerbaikan { get; set; }

        public bool BrConfirm { get; set; }
        public bool SarmConfirm { get; set; }
        public bool EcsConfirm { get; set; }

        public StatusRic Status { get; set; } = StatusRic.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public Group? Group { get; set; }
    }
}
