using Core.Models.Enums;
using Newtonsoft.Json;

namespace Core.RequestModels.Ric
{
    public class FormRicDetailResponse
    {
        public Guid Id { get; set; }
        public Guid IdUser { get; set; }
        public Guid IdGroupUser { get; set; }
        public string? Judul { get; set; }

        public List<string> Hastag { get; set; } = new();
        public List<string> AsIsProcessRasciFile { get; set; } = new();

        public string? Permasalahan { get; set; }
        public string? DampakMasalah { get; set; }
        public string? FaktorPenyebabMasalah { get; set; }
        public string? SolusiSaatIni { get; set; }

        public List<string> AlternatifSolusi { get; set; } = new();
        public List<string> ToBeProcessBusinessRasciKkiFile { get; set; } = new();

        public string? PotensiValueCreation { get; set; }
        public List<string> ExcpectedCompletionTargetFile { get; set; } = new(); // nullable string sesuai contoh
        public string? HasilSetelahPerbaikan { get; set; }

        public StatusRic Status { get; set; }
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("reviews")]
        public List<ReviewRicResponse> Reviews { get; set; } = new();

        [JsonProperty("histories")]
        public List<RicHistoryResponse> Histories { get; set; } = new();
    }

    public class ReviewRicResponse
    {
        public Guid Id { get; set; }
        public string? Catatan { get; set; }
        public string RoleReview { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RicHistoryResponse
    {
        public int Version { get; set; }
        public string Snapshot { get; set; } = string.Empty;
        public string? EditedFields { get; set; }
        public string? EditorName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
