using System.ComponentModel.DataAnnotations;
using Core.Models.Enums;

namespace Core.RequestModels
{
    public class RejectRicRequest
    {
        [Required]
        public string Catatan { get; set; } = default!;

        // [Required]
        // public RoleReview RoleReview { get; set; }
    }
}
