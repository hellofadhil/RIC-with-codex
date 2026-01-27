using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnePro.API.Interfaces;

namespace OnePro.API.Controllers.V1
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupController : ControllerBase
    {
        private readonly IGroupRepository _repo;

        public GroupController(IGroupRepository repo)
        {
            _repo = repo;
        }

        // ============================================================
        // GET CURRENT USER GROUP + MEMBERS
        // ============================================================
        [HttpGet("my")]
        public async Task<IActionResult> GetMyGroup()
        {
            var groupIdStr = User.FindFirstValue("groupId");

            if (string.IsNullOrEmpty(groupIdStr))
                return BadRequest("User does not belong to any group.");

            var groupId = Guid.Parse(groupIdStr);

            var result = await _repo.GetGroupWithMembersAsync(groupId);

            if (result == null)
                return NotFound("Group not found.");

            return Ok(result);
        }

    }
}
