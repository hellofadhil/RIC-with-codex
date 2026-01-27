using OnePro.Front.Models;

namespace OnePro.Front.Services.Interfaces
{
    public interface IGroupService
    {
        Task<GroupResponse?> GetMyGroupAsync(string token);
    }
}
