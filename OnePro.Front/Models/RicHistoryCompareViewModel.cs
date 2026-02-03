using OnePro.Front.Models;

namespace OnePro.Front.Models
{
    public class RicHistoryCompareViewModel
    {
        public Guid RicId { get; set; }
        public RicHistoryResponse Current { get; set; } = default!;
        public RicHistoryResponse? Previous { get; set; }
        public string Title { get; set; } = "RIC History Compare";
        public string BackUrl { get; set; } = "/Ric/User";
    }
}
