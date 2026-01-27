using Microsoft.AspNetCore.Mvc;
using OnePro.Front.Middleware;
using OnePro.Front.Services.Interfaces;

namespace OnePro.Front.Controllers
{
    [AuthRequired]
    public class GroupController : Controller
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        public async Task<IActionResult> Index()
        {
            string token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var group = await _groupService.GetMyGroupAsync(token);

            return View(group);
        }

        // [HttpPost]
        // public async Task<IActionResult> Invite(string email, int role)
        // {
        //     var token = HttpContext.Session.GetString("JwtToken");

        //     await _groupService.InviteAsync(token, email, role);

        //     return RedirectToAction("Index");
        // }

        // [HttpPost]
        // public async Task<IActionResult> UpdateRole(Guid id, int role)
        // {
        //     var token = HttpContext.Session.GetString("JwtToken");

        //     await _groupService.UpdateRoleAsync(token, id, role);

        //     return RedirectToAction("Index");
        // }

        // [HttpPost]
        // public async Task<IActionResult> Delete(Guid id)
        // {
        //     var token = HttpContext.Session.GetString("JwtToken");

        //     await _groupService.DeleteMemberAsync(token, id);

        //     return RedirectToAction("Index");
        // }
    }
}
