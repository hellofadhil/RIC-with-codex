using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Core.Models.Enums;

namespace OnePro.Front.Models.RicRollOut
{
    public class RicRollOutDetailViewModel
    {
        // ===== ID =====
        public Guid Id { get; set; }

        // ===== READ-ONLY DETAIL (buat header/info) =====
        public Guid IdUser { get; set; }
        public string UserName { get; set; } = default!;
        public Guid IdGroupUser { get; set; }
        public string GroupName { get; set; } = default!;
        public StatusRicRollOut Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ===== EDITABLE FORM =====
        [Required]
        public string? Entitas { get; set; }

        [Required]
        public string? JudulAplikasi { get; set; }

        public List<string> Hashtags { get; set; } = new();

        // upload baru
        public IEnumerable<IFormFile>? CompareWithAsIsHoldingProcessFiles { get; set; }
        public IEnumerable<IFormFile>? StkAsIsToBeFiles { get; set; }

        // existing file urls (buat keep file lama)
        public List<string> ExistingCompareWithAsIsHoldingProcessFileUrls { get; set; } = new();
        public List<string> ExistingStkAsIsToBeFileUrls { get; set; } = new();

        // checklist
        public bool IsJoinedDomainAdPertamina { get; set; }
        public bool IsUsingErpPertamina { get; set; }
        public bool IsImplementedRequiredActivation { get; set; }
        public bool HasDataCenterConnection { get; set; }
        public bool HasRequiredResource { get; set; }

        // ===== EXTRA DISPLAY =====
        public List<RicRollOutReviewItemViewModel> Reviews { get; set; } = new();
        public List<RicRollOutHistoryItemViewModel> Histories { get; set; } = new();
    }

    public class RicRollOutReviewItemViewModel
    {
        public string? UserName { get; set; }
        public string? RoleReview { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Catatan { get; set; }
    }

    public class RicRollOutHistoryItemViewModel
    {
        public DateTime CreatedAt { get; set; }
        public string Title { get; set; } = default!;     // contoh: "Submitted", "Rejected", "Resubmitted"
        public string? Description { get; set; }          // contoh: "Rejected by ECS_PIC: ... "
        public string? ActorName { get; set; }            // siapa yg melakukan aksi
        public string? ActorRole { get; set; }            // role actor
        public StatusRicRollOut? FromStatus { get; set; }
        public StatusRicRollOut? ToStatus { get; set; }
    }
}
