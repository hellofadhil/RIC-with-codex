using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models.Entities
{
    public class Group
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string NamaDivisi { get; set; } = default!;
        public string NamaPerusahaan { get; set; } = default!;

        public List<User>? Users { get; set; }
        public List<FormRic>? FormRics { get; set; }
    }
}
