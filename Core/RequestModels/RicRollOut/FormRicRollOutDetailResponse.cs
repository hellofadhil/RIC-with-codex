// using Core.Models.Enums;

// namespace Core.RequestModels.RicRollOut
// {
//     public class FormRicRollOutDetailResponse
//     {
//         public Guid Id { get; set; }

//         // OWNER
//         public Guid IdUser { get; set; }
//         public string UserName { get; set; } = default!;
//         public Guid IdGroupUser { get; set; }
//         public string GroupName { get; set; } = default!;

//         // MAIN DATA
//         public string Entitas { get; set; } = default!;
//         public string JudulAplikasi { get; set; } = default!;
//         public List<string>? Hashtag { get; set; }

//         // FILES
//         public List<string>? CompareWithAsIsHoldingProcessFiles { get; set; }
//         public List<string>? StkAsIsToBeFiles { get; set; }

//         // CHECKLIST
//         public bool IsJoinedDomainAdPertamina { get; set; }
//         public bool IsUsingErpPertamina { get; set; }
//         public bool IsImplementedRequiredActivation { get; set; }
//         public bool HasDataCenterConnection { get; set; }
//         public bool HasRequiredResource { get; set; }

//         // STATUS
//         public StatusRicRollOut Status { get; set; }

//         // META
//         public DateTime CreatedAt { get; set; }
//         public DateTime UpdatedAt { get; set; }
//     }
// }

using Core.Models.Enums;
using Newtonsoft.Json;

namespace Core.RequestModels.RicRollOut
{
    public class FormRicRollOutDetailResponse
    {
        // ===== ID =====
        public Guid Id { get; set; }

        // ===== OWNER =====
        public Guid IdUser { get; set; }
        public string? UserName { get; set; }

        public Guid IdGroupUser { get; set; }
        public string? GroupName { get; set; }

        // ===== MAIN DATA =====
        public string Entitas { get; set; } = default!;
        public string JudulAplikasi { get; set; } = default!;

        public List<string> Hashtag { get; set; } = new();

        // ===== FILES =====
        public List<string> CompareWithAsIsHoldingProcessFiles { get; set; } = new();
        public List<string> StkAsIsToBeFiles { get; set; } = new();

        // ===== CHECKLIST =====
        public bool IsJoinedDomainAdPertamina { get; set; }
        public bool IsUsingErpPertamina { get; set; }
        public bool IsImplementedRequiredActivation { get; set; }
        public bool HasDataCenterConnection { get; set; }
        public bool HasRequiredResource { get; set; }

        // ===== STATUS =====
        public StatusRicRollOut Status { get; set; }

        // ===== META =====
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ===== EXTRA =====
        [JsonProperty("reviews")]
        public List<RicRollOutReviewResponse> Reviews { get; set; } = new();

        [JsonProperty("histories")]
        public List<RicRollOutHistoryResponse> Histories { get; set; } = new();
    }

    // ================= REVIEW =================
    public class RicRollOutReviewResponse
    {
        public Guid Id { get; set; }
        public string? Catatan { get; set; }
        public string RoleReview { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ================= HISTORY =================
    public class RicRollOutHistoryResponse
    {
        public int Version { get; set; }
        public string Action { get; set; } = string.Empty; // Created, Submitted, Rejected, Resubmitted
        public string? Description { get; set; }
        public string? ActorName { get; set; }
        public string? ActorRole { get; set; }
        public StatusRicRollOut? FromStatus { get; set; }
        public StatusRicRollOut? ToStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
