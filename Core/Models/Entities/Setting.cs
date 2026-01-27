using System.ComponentModel.DataAnnotations.Schema;
using Core.Models.Abstracts;
using Core.Models.Interfaces;

namespace Core.Models.Entities
{
    [Table("Portal.Settings")]
    public class Setting : AuditableEntity
    {
        public string? Config { get; set; } = default!;
        public string? Name { get; set; } = default!;
        public string? Value { get; set; } = default!;
    }
}
