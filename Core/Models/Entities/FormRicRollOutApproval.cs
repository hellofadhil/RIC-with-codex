using System.ComponentModel.DataAnnotations;
using Core.Models.Enums;

namespace Core.Models.Entities
{
    public class FormRicRollOutApproval
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid IdFormRicRollOut { get; set; }
        public Guid IdApprover { get; set; }

        public RoleApproval Role { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

        public DateTime? ApprovalDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public FormRicRollOut? FormRicRollOut { get; set; }
        public User? Approver { get; set; }
    }
}
