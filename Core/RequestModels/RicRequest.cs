using Core.Models.Enums;

namespace Core.RequestModels
{
    public class FormRicRequest
    {
        public required string Judul { get; set; } = default!;

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
        public StatusRic Status { get; set; }
    }
}
