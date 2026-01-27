using System.ComponentModel.DataAnnotations;

namespace Core.Models.Entities
{
    public class UndanganFormRic
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid IdBr { get; set; }
        public Guid IdUser { get; set; }
        public Guid IdGroupUser { get; set; }

        public string EmailUser { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public string Content { get; set; } = default!;
        public string? Link { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? Br { get; set; }
        public User? User { get; set; }
        public Group? Group { get; set; }
    }
}
