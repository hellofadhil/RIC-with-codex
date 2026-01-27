using Core.Models.Entities;

namespace Core.RequestModels
{
    public class SettingRequest : Setting
    {
        public string Type { get; set; } = default!;
    }
}
