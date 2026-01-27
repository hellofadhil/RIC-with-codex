namespace Core.ViewModels
{
    public class ReviewFormRicResponse
    {
        public Guid Id { get; set; }

        public Guid IdFormRic { get; set; }
        public Guid IdUser { get; set; }

        public string? Catatan { get; set; }
        public string RoleReview { get; set; } = default!;

        public DateTime CreatedAt { get; set; }
    }
}
