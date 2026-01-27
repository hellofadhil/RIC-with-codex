namespace Core.ViewModels
{
    public class FormRicApprovalResponse
    {
        public Guid Id { get; set; }
        public Guid IdFormRic { get; set; }
        public Guid IdApprover { get; set; }

        public string Role { get; set; } = default!;
        public string ApprovalStatus { get; set; } = default!;

        public DateTime? ApprovalDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
