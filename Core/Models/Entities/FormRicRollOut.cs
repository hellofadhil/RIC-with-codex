using System.ComponentModel.DataAnnotations;
using Core.Models.Enums;

namespace Core.Models.Entities
{
    public class FormRicRollOut
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid IdUser { get; set; }
        public Guid IdGroupUser { get; set; }

        public string Entitas { get; set; } = default!;
        public string JudulAplikasi { get; set; } = default!;

        public List<string>? Hashtag { get; set; }
        public List<string>? CompareWithAsIsHoldingProcessFiles { get; set; }
        public List<string>? StkAsIsToBeFiles { get; set; }

        public bool IsJoinedDomainAdPertamina { get; set; }
        public bool IsUsingErpPertamina { get; set; }
        public bool IsImplementedRequiredActivation { get; set; }
        public bool HasDataCenterConnection { get; set; }
        public bool HasRequiredResource { get; set; }

        public StatusRicRollOut Status { get; set; } = StatusRicRollOut.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public Group? Group { get; set; }

        public List<FormRicRollOutApproval>? Approvals { get; set; }
        public List<FormRicRollOutHistory>? Histories { get; set; }
        public List<ReviewFormRicRollOut>? Reviews { get; set; }
    }
}
