using System.ComponentModel.DataAnnotations;
using Core.Models.Enums;

namespace Core.Models.Entities
{
    public class ReviewFormRic
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid IdFormRic { get; set; }
        public Guid IdUser { get; set; }

        public string? Catatan { get; set; }
        public RoleReview RoleReview { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public FormRic? FormRic { get; set; }
        public User? User { get; set; }
    }
}
