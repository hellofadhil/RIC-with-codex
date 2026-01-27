using System.ComponentModel.DataAnnotations;
using Core.Models.Enums;

namespace Core.Models.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? IdGroup { get; set; }

        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;

        public Role Role { get; set; }
        

        public string Position { get; set; } = default!;
        public string? TandaTanganFile { get; set; }

        public string PasswordHash { get; set; } = default!;

        public Group? Group { get; set; }
    }
}
