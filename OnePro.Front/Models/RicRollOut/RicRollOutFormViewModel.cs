using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace OnePro.Front.Models.RicRollOut
{
    public class RicRollOutFormViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string? Entitas { get; set; }

        [Required]
        public string? JudulAplikasi { get; set; }

        public List<string>? Hashtags { get; set; } = new();

        // upload baru
        public IEnumerable<IFormFile>? CompareWithAsIsHoldingProcessFiles { get; set; }
        public IEnumerable<IFormFile>? StkAsIsToBeFiles { get; set; }

        // existing (buat edit/update)
        public List<string>? ExistingCompareWithAsIsHoldingProcessFileUrls { get; set; } = new();
        public List<string>? ExistingStkAsIsToBeFileUrls { get; set; } = new();

        // checklist
        public bool IsJoinedDomainAdPertamina { get; set; }
        public bool IsUsingErpPertamina { get; set; }
        public bool IsImplementedRequiredActivation { get; set; }
        public bool HasDataCenterConnection { get; set; }
        public bool HasRequiredResource { get; set; }
    }
}
