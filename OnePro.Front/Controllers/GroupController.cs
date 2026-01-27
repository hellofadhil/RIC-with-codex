using Microsoft.AspNetCore.Mvc;
using Core.Models.Enums;
using OnePro.Front.Middleware;
using OnePro.Front.Services.Interfaces;
using Core.RequestModels.Group;

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

            if (group == null)
                return View("Create");

            return View(group);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGroupRequest request)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var result = await _groupService.CreateGroupAsync(token, request);
            if (result == null)
            {
                ViewBag.Error = "Gagal membuat divisi.";
                return View();
            }

            HttpContext.Session.SetString("JwtToken", result.Token);
            HttpContext.Session.SetString("UserName", result.User.Name ?? "");
            HttpContext.Session.SetString("UserEmail", result.User.Email ?? "");
            HttpContext.Session.SetString("UserRole", result.User.RoleName ?? "");

            return RedirectToAction("Index");
        }

        [RoleRequired(Role.User_Pic, Role.BR_Pic, Role.SARM_Pic, Role.ECS_Pic)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Invite(AddGroupMemberRequest request)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            await _groupService.AddMemberAsync(token, request);
            return RedirectToAction("Index");
        }

        [RoleRequired(Role.User_Pic, Role.BR_Pic, Role.SARM_Pic, Role.ECS_Pic)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(Guid id, int role)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            await _groupService.UpdateRoleAsync(token, id, role);
            return RedirectToAction("Index");
        }

        [RoleRequired(Role.User_Pic, Role.BR_Pic, Role.SARM_Pic, Role.ECS_Pic)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            await _groupService.DeleteMemberAsync(token, id);
            return RedirectToAction("Index");
        }
    }
}
