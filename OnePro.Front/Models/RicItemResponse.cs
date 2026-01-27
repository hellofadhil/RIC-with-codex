using Core.Models.Entities;
using Core.Models.Enums;
using Newtonsoft.Json;

namespace OnePro.Front.Models
{
    public class RicItemResponse
    {
        public Guid Id { get; set; }
        public string Judul { get; set; } = default!;
        public string? Permasalahan { get; set; }
        public string? UserName { get; set; }
        public string? Status { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Include hashtag from API for list/search.
        public List<string>? Hastag { get; set; }
    }

    public class RicCreateViewModel
    {
        public Guid Id { get; set; }

        // Basic fields
        public string JudulPermintaan { get; set; } = string.Empty;
        public List<string> Hashtags { get; set; } = new();
        public string Permasalahan { get; set; } = string.Empty;
        public string DampakMasalah { get; set; } = string.Empty;
        public string FaktorPenyebab { get; set; } = string.Empty;
        public string SolusiSaatIni { get; set; } = string.Empty;
        public List<string> Alternatifs { get; set; } = new();
        public string? PotentialValue { get; set; }
        public string HasilSetelahPerbaikan { get; set; } = string.Empty;

        public List<IFormFile>? AsIsRasciFiles { get; set; }
        public List<IFormFile>? ToBeProcessFiles { get; set; }
        public List<IFormFile>? ExpectedCompletionFiles { get; set; }

        // Existing URLs (for edit mode)
        public List<string>? ExistingAsIsFileUrls { get; set; }
        public List<string>? ExistingToBeFileUrls { get; set; }
        public List<string>? ExistingExpectedCompletionFileUrls { get; set; }

        public List<ReviewRicResponse> Reviews { get; set; } = new();
        public List<RicHistoryResponse> Histories { get; set; } = new();
    }

    public class FormRicCreateRequest
    {
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
        public int Status { get; set; }
    }

    // === Tambahan ini yang lagi dicari compiler ===

    // public class RicDetailResponse
    // {
    //     public Guid Id { get; set; }

    //     public string Judul { get; set; } = default!;
    //     public List<string>? Hastag { get; set; }
    //     public List<string>? AsIsProcessRasciFile { get; set; }

    //     public string? Permasalahan { get; set; }
    //     public string? DampakMasalah { get; set; }
    //     public string? FaktorPenyebabMasalah { get; set; }
    //     public string? SolusiSaatIni { get; set; }

    //     public List<string>? AlternatifSolusi { get; set; }
    //     public List<string>? ToBeProcessBusinessRasciKkiFile { get; set; }
    //     public string? PotensiValueCreation { get; set; }

    //     public List<string>? ExcpectedCompletionTargetFile { get; set; }

    //     [JsonProperty("hasilSetelahPerbaikan")]
    //     public string? HasilSetelahPerbaikan { get; set; }

    //     public int Status { get; set; }
    // }

    public class RicDetailResponse
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

        [JsonProperty("hasilSetelahPerbaikan")]
        public string? HasilSetelahPerbaikan { get; set; }

        public int Status { get; set; }

        // Include reviews + histories
        [JsonProperty("reviews")]
        public List<ReviewRicResponse>? Reviews { get; set; }

        [JsonProperty("histories")]
        public List<RicHistoryResponse>? Histories { get; set; }
    }

    public class FormRicUpdateRequest
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
        public int Status { get; set; }
    }

    public class RicReviewRequest
    {
        public string Action { get; set; } = default!; // "Approve" / "Reject"
        public string? Note { get; set; } // catatan BR
    }

    public class FormRicResubmitRequest
    {
        public string? Judul { get; set; }
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

        public int Status { get; set; }
    }

    public class ReviewRicResponse
    {
        public Guid Id { get; set; }

        [JsonProperty("catatan")]
        public string? Catatan { get; set; }

        [JsonProperty("roleReview")]
        public string? RoleReview { get; set; }

        [JsonProperty("userName")]
        public string? UserName { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class RicHistoryResponse
    {
        public Guid Id { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("editorName")]
        public string? EditorName { get; set; }

        [JsonProperty("editedFields")]
        public string? EditedFields { get; set; }

        [JsonProperty("snapshot")]
        public string? Snapshot { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
