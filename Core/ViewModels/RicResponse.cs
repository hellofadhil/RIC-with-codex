namespace Core.ViewModels
{
    public class RicResponse
    {
        public Guid Id { get; set; }

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

        public string Status { get; set; } = default!;
    }
}
