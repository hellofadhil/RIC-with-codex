using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Models.Enums;
using Core.RequestModels.Group;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OnePro.API.Auth;
using OnePro.API.Interfaces;

namespace OnePro.API.Controllers.V1
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupController : ControllerBase
    {
        private readonly IGroupRepository _repo;
        private readonly IConfiguration _config;

        public GroupController(IGroupRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        private static readonly Guid GroupUserId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        private static readonly Guid GroupBrId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        private static readonly Guid GroupSarmId = Guid.Parse("30000000-0000-0000-0000-000000000003");
        private static readonly Guid GroupEcsId = Guid.Parse("40000000-0000-0000-0000-000000000004");

        private Guid GetGuidClaim(string key)
        {
            var value = User.FindFirstValue(key);
            if (!Guid.TryParse(value, out var result))
                throw new InvalidOperationException($"{key} missing in token");
            return result;
        }

        private Role GetRoleClaim()
        {
            var roleStr = User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role);
            if (!Enum.TryParse(roleStr, ignoreCase: true, out Role role))
                throw new InvalidOperationException("role missing in token");
            return role;
        }

        private static bool IsPic(Role role) => role.ToString().EndsWith("_Pic", StringComparison.Ordinal);

        private static bool RoleAllowedForGroup(Role role, Guid groupId)
        {
            if (
                groupId != GroupUserId
                && groupId != GroupBrId
                && groupId != GroupSarmId
                && groupId != GroupEcsId
            )
            {
                return role is Role.User_Member or Role.User_Pic or Role.User_Manager or Role.User_VP;
            }

            return groupId switch
            {
                _ when groupId == GroupUserId =>
                    role is Role.User_Member or Role.User_Pic or Role.User_Manager or Role.User_VP,
                _ when groupId == GroupBrId =>
                    role is Role.BR_Pic or Role.BR_Member or Role.BR_Manager or Role.BR_VP,
                _ when groupId == GroupSarmId =>
                    role is Role.SARM_Pic or Role.SARM_Member or Role.SARM_Manager or Role.SARM_VP,
                _ when groupId == GroupEcsId =>
                    role is Role.ECS_Pic or Role.ECS_Member or Role.ECS_Manager or Role.ECS_VP,
                _ => false,
            };
        }

        private static bool TryGetGuidClaim(ClaimsPrincipal user, string key, out Guid result)
        {
            result = Guid.Empty;
            var value = user.FindFirstValue(key);
            return Guid.TryParse(value, out result) && result != Guid.Empty;
        }

        private string GenerateJwtToken(Core.Models.Entities.User user)
        {
            string key = _config["Key:Jwt"] ?? throw new Exception("Missing Key:Jwt");

            var claims = new List<Claim>
            {
                new Claim("id", user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("name", user.Name),
                new Claim("role", user.Role.ToString()),
            };

            if (user.IdGroup.HasValue && user.IdGroup.Value != Guid.Empty)
                claims.Add(new Claim("groupId", user.IdGroup.Value.ToString()));

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.UtcNow.AddMinutes(60),
                claims: claims,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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

        // ============================================================
        // CREATE GROUP (User without group)
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.NamaDivisi) || string.IsNullOrWhiteSpace(req.NamaPerusahaan))
                return BadRequest("Nama divisi dan perusahaan wajib diisi.");

            if (!TryGetGuidClaim(User, "id", out var userId))
                return Unauthorized("User not found.");

            var user = await _repo.GetUserByIdAsync(userId);
            if (user == null)
                return Unauthorized("User not found.");

            if (user.IdGroup.HasValue && user.IdGroup.Value != Guid.Empty)
                return BadRequest("User already belongs to a group.");

            var group = new Core.Models.Entities.Group
            {
                Id = Guid.NewGuid(),
                NamaDivisi = req.NamaDivisi.Trim(),
                NamaPerusahaan = req.NamaPerusahaan.Trim(),
            };

            var groupOk = await _repo.CreateGroupAsync(group);
            if (!groupOk)
                return StatusCode(500, "Failed to create group.");

            user.IdGroup = group.Id;
            user.Role = Role.User_Pic;

            var userOk = await _repo.UpdateMemberAsync(user);
            if (!userOk)
                return StatusCode(500, "Failed to update user group.");

            return Ok(
                new
                {
                    message = "Group created",
                    group = new { group.Id, group.NamaDivisi, group.NamaPerusahaan },
                    user = new
                    {
                        user.Id,
                        user.Email,
                        user.Name,
                        user.Role,
                        user.IdGroup,
                    },
                    token = GenerateJwtToken(user),
                }
            );
        }

        // ============================================================
        // UPDATE GROUP (PIC only)
        // ============================================================
        [HttpPut("my")]
        [RoleRequired(Role.User_Pic, Role.BR_Pic, Role.SARM_Pic, Role.ECS_Pic)]
        public async Task<IActionResult> UpdateMyGroup([FromBody] UpdateGroupRequest req)
        {
            if (!TryGetGuidClaim(User, "groupId", out var groupId))
                return BadRequest("User does not belong to any group.");

            if (string.IsNullOrWhiteSpace(req.NamaDivisi) || string.IsNullOrWhiteSpace(req.NamaPerusahaan))
                return BadRequest("Nama divisi dan perusahaan wajib diisi.");

            var group = await _repo.GetGroupByIdAsync(groupId);
            if (group == null)
                return NotFound("Group not found.");

            group.NamaDivisi = req.NamaDivisi.Trim();
            group.NamaPerusahaan = req.NamaPerusahaan.Trim();

            var ok = await _repo.UpdateGroupAsync(group);
            return ok ? Ok(group) : StatusCode(500, "Failed to update group.");
        }

        // ============================================================
        // ADD MEMBER (PIC only)
        // ============================================================
        [HttpPost("members")]
        [RoleRequired(Role.User_Pic, Role.BR_Pic, Role.SARM_Pic, Role.ECS_Pic)]
        public async Task<IActionResult> AddMember([FromBody] AddGroupMemberRequest req)
        {
            if (!TryGetGuidClaim(User, "groupId", out var groupId))
                return BadRequest("User does not belong to any group.");
            var editorId = GetGuidClaim("id");
            var editorRole = GetRoleClaim();

            if (!IsPic(editorRole))
                return Forbid();

            if (!RoleAllowedForGroup(editorRole, groupId))
                return Forbid();

            if (!RoleAllowedForGroup(req.Role, groupId))
                return BadRequest("Role is not allowed for this group.");

            if (string.IsNullOrWhiteSpace(req.Email))
                return BadRequest("Email is required.");

            var user = await _repo.GetUserByEmailAsync(req.Email.Trim());
            if (user == null)
                return BadRequest("Email not found in system.");

            if (user.IdGroup.HasValue && user.IdGroup.Value != Guid.Empty)
                return BadRequest("User already belongs to another group.");

            user.IdGroup = groupId;
            user.Role = req.Role;

            var ok = await _repo.UpdateMemberAsync(user);
            if (!ok)
                return StatusCode(500, "Failed to add member.");

            return Ok(new { user.Id, user.Email, user.Name, user.Position, Role = user.Role.ToString() });
        }

        // ============================================================
        // UPDATE ROLE (PIC only)
        // ============================================================
        [HttpPut("members/{id:guid}/role")]
        [RoleRequired(Role.User_Pic, Role.BR_Pic, Role.SARM_Pic, Role.ECS_Pic)]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateMemberRoleRequest req)
        {
            if (!TryGetGuidClaim(User, "groupId", out var groupId))
                return BadRequest("User does not belong to any group.");
            var editorRole = GetRoleClaim();

            if (!IsPic(editorRole))
                return Forbid();

            if (!RoleAllowedForGroup(editorRole, groupId))
                return Forbid();

            if (!RoleAllowedForGroup(req.Role, groupId))
                return BadRequest("Role is not allowed for this group.");

            var user = await _repo.GetUserByIdAsync(id);
            if (user == null)
                return NotFound("Member not found.");

            if (user.IdGroup != groupId)
                return Forbid();

            var ok = await _repo.UpdateMemberRoleAsync(user, req.Role);
            return ok ? NoContent() : StatusCode(500, "Failed to update role.");
        }

        // ============================================================
        // DELETE MEMBER (PIC only, cannot delete PIC)
        // ============================================================
        [HttpDelete("members/{id:guid}")]
        [RoleRequired(Role.User_Pic, Role.BR_Pic, Role.SARM_Pic, Role.ECS_Pic)]
        public async Task<IActionResult> DeleteMember(Guid id)
        {
            if (!TryGetGuidClaim(User, "groupId", out var groupId))
                return BadRequest("User does not belong to any group.");
            var editorId = GetGuidClaim("id");
            var editorRole = GetRoleClaim();

            if (!IsPic(editorRole))
                return Forbid();

            if (!RoleAllowedForGroup(editorRole, groupId))
                return Forbid();

            if (id == editorId)
                return BadRequest("Cannot delete your own account.");

            var user = await _repo.GetUserByIdAsync(id);
            if (user == null)
                return NotFound("Member not found.");

            if (user.IdGroup != groupId)
                return Forbid();

            if (IsPic(user.Role))
                return BadRequest("Cannot delete PIC member.");

            var ok = await _repo.DeleteMemberAsync(user);
            return ok ? NoContent() : StatusCode(500, "Failed to delete member.");
        }

        // ============================================================
        // DELETE GROUP (User_Pic only, no other members)
        // ============================================================
        [HttpDelete("my")]
        [RoleRequired(Role.User_Pic)]
        public async Task<IActionResult> DeleteMyGroup()
        {
            if (!TryGetGuidClaim(User, "groupId", out var groupId))
                return BadRequest("User does not belong to any group.");

            var userId = GetGuidClaim("id");
            var role = GetRoleClaim();
            if (role != Role.User_Pic)
                return Forbid();

            var memberCount = await _repo.CountMembersAsync(groupId);
            if (memberCount > 1)
                return BadRequest("Group still has members.");

            var group = await _repo.GetGroupByIdAsync(groupId);
            if (group == null)
                return NotFound("Group not found.");

            var user = await _repo.GetUserByIdAsync(userId);
            if (user == null)
                return Unauthorized("User not found.");

            user.IdGroup = null;
            user.Role = Role.User_Member;

            var userOk = await _repo.UpdateMemberAsync(user);
            if (!userOk)
                return StatusCode(500, "Failed to update user.");

            var groupOk = await _repo.DeleteGroupAsync(group);
            if (!groupOk)
                return StatusCode(500, "Failed to delete group.");

            return Ok(new { message = "Group deleted", token = GenerateJwtToken(user) });
        }
    }
}
