using System.Security.Claims;
using Core.Models;
using Core.Models.Entities;
using Core.Models.Enums;
using Core.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnePro.API.Auth;
using OnePro.API.Interfaces;

namespace OnePro.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RicController : ControllerBase
{
    private readonly IRicRepository _repository;
    private readonly OneProDbContext _context;

    public RicController(IRicRepository repository, OneProDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    // =========================
    // CLAIM HELPERS
    // =========================
    private Guid GetGuidClaim(string key)
    {
        var value = User.FindFirstValue(key);
        if (!Guid.TryParse(value, out var result))
            throw new InvalidOperationException($"{key} missing in token");
        return result;
    }

    private string GetStringClaim(string key)
    {
        // exact match (custom claims)
        var value = User.FindFirstValue(key);

        // fallbacks (common claim mappings)
        if (
            string.IsNullOrWhiteSpace(value)
            && key.Equals("role", StringComparison.OrdinalIgnoreCase)
        )
            value = User.FindFirstValue(ClaimTypes.Role);

        if (
            string.IsNullOrWhiteSpace(value)
            && key.Equals("name", StringComparison.OrdinalIgnoreCase)
        )
            value = User.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{key} missing in token");

        return value;
    }

    // =========================
    // MAPPING HELPERS
    // =========================
    private static void ApplyRequestToEntity(FormRic ric, FormRicRequest req)
    {
        ric.Judul = req.Judul;
        ric.Hastag = req.Hastag;
        ric.AsIsProcessRasciFile = req.AsIsProcessRasciFile;
        ric.Permasalahan = req.Permasalahan;
        ric.DampakMasalah = req.DampakMasalah;
        ric.FaktorPenyebabMasalah = req.FaktorPenyebabMasalah;
        ric.SolusiSaatIni = req.SolusiSaatIni;
        ric.AlternatifSolusi = req.AlternatifSolusi;
        ric.ToBeProcessBusinessRasciKkiFile = req.ToBeProcessBusinessRasciKkiFile;
        ric.PotensiValueCreation = req.PotensiValueCreation;
        ric.ExcpectedCompletionTargetFile = req.ExcpectedCompletionTargetFile;
        ric.HasilSetelahPerbaikan = req.HasilSetelahPerbaikan;
    }

    private static void Touch(FormRic ric) => ric.UpdatedAt = DateTime.UtcNow;

    // =========================
    // QUERIES
    // =========================
    [HttpGet("my")]
    public async Task<IActionResult> GetMyGroupRics()
    {
        var groupId = GetGuidClaim("groupId");
        var data = await _repository.GetAllByGroupAsync(groupId);
        return Ok(data);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ric = await _repository.GetByIdAsync(id);
        return ric is null ? NotFound("RIC not found.") : Ok(ric);
    }

    [HttpGet("{id:guid}/detail")]
    public async Task<IActionResult> GetDetailById(Guid id)
    {
        var ric = await _repository.GetDetailByIdAsync(id);
        return ric is null ? NotFound("RIC not found.") : Ok(ric);
    }

    // =========================
    // COMMANDS
    // =========================
    [RoleRequired(Role.User_Pic)]
    [HttpPost]
    // [Authorize(Roles = "User_Pic,BR_Pic,SARM_Pic")]
    public async Task<IActionResult> Create([FromBody] FormRicRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ric = new FormRic
        {
            IdUser = GetGuidClaim("id"),
            IdGroupUser = GetGuidClaim("groupId"),
            Status = req.Status,

            BrConfirm = false,
            SarmConfirm = false,
            EcsConfirm = false,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        ApplyRequestToEntity(ric, req);

        var ok = await _repository.CreateAsync(ric);
        return ok
            ? CreatedAtAction(nameof(GetById), new { id = ric.Id }, ric)
            : StatusCode(500, "Failed to create RIC.");
    }

    [RoleRequired(Role.User_Pic)]
    [HttpPut("{id:guid}")]
    // [Authorize(Roles = "User_Pic,BR_Pic,SARM_Pic")]
    public async Task<IActionResult> Update(Guid id, [FromBody] FormRicRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var groupId = GetGuidClaim("groupId");
        var ric = await _repository.GetByIdAsync(id);

        if (ric is null)
            return NotFound("RIC not found.");

        if (ric.IdGroupUser != groupId)
            return Forbid();

        ApplyRequestToEntity(ric, req);
        ric.Status = req.Status;
        Touch(ric);

        var ok = await _repository.UpdateAsync(ric);
        return ok ? NoContent() : StatusCode(500, "Failed to update RIC.");
    }

    [RoleRequired(Role.User_Pic)]
    [HttpPut("{id:guid}/resubmit")]
    // [Authorize(Roles = "User_Pic,BR_Pic,SARM_Pic")]
    public async Task<IActionResult> ResubmitAfterRejection(Guid id, [FromBody] FormRicRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var groupId = GetGuidClaim("groupId");
        var editorId = GetGuidClaim("id");

        var ric = await _repository.GetByIdAsync(id);
        if (ric is null)
            return NotFound("RIC not found.");

        if (ric.IdGroupUser != groupId)
            return Forbid();

        ApplyRequestToEntity(ric, req);
        ric.Status = StatusRic.Review_BR;
        Touch(ric);

        var ok = await _repository.ResubmitAfterRejection(ric, editorId);
        return ok ? NoContent() : StatusCode(500, "Failed to update RIC.");
    }

    [RoleRequired(Role.BR_Pic, Role.SARM_Pic, Role.ECS_Pic)]
    [HttpPut("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRicRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetGuidClaim("id");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return Unauthorized("User not found.");

        // role -> (reviewRole, newStatus)
        var rejectMap = new Dictionary<Role, (RoleReview ReviewRole, StatusRic NewStatus)>
        {
            [Role.BR_Pic] = (RoleReview.BR, StatusRic.Return_BR_To_User),
            [Role.SARM_Pic] = (RoleReview.SARM, StatusRic.Return_SARM_To_BR),
            [Role.ECS_Pic] = (RoleReview.ECS, StatusRic.Return_ECS_To_BR),
        };

        if (!rejectMap.TryGetValue(user.Role, out var cfg))
            return Forbid();

        var ric = await _repository.GetByIdAsync(id);
        if (ric is null)
            return NotFound("RIC not found.");

        ric.Status = cfg.NewStatus;
        Touch(ric);

        await _repository.AddReviewAsync(
            new ReviewFormRic
            {
                IdFormRic = ric.Id,
                IdUser = userId,
                Catatan = req.Catatan,
                RoleReview = cfg.ReviewRole,
                CreatedAt = DateTime.UtcNow,
            }
        );

        await _repository.UpdateAsync(ric);

        return Ok(
            new
            {
                message = "RIC rejected",
                status = ric.Status.ToString(),
                reviewer = cfg.ReviewRole.ToString(),
            }
        );
    }

    [RoleRequired(Role.BR_Pic, Role.SARM_Pic, Role.ECS_Pic)]
    [HttpPut("{id:guid}/forward")]
    // [Authorize(Roles = "BR_Pic,SARM_Pic,ECS_Pic")]
    public async Task<IActionResult> Forward(Guid id, [FromBody] FormRicRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var editorId = GetGuidClaim("id");
        var roleStr = GetStringClaim("role");

        if (!Enum.TryParse(roleStr, out Role role))
            return Forbid("Invalid role.");

        var ric = await _repository.GetByIdAsync(id);
        if (ric is null)
            return NotFound("RIC not found.");

        // kalau forward memang membawa revisi data
        ApplyRequestToEntity(ric, req);
        Touch(ric);

        // transitions + guards
        switch (role)
        {
            case Role.BR_Pic:
                if (
                    ric.Status
                    is not (
                        StatusRic.Submitted_To_BR
                        or StatusRic.Review_BR
                        or StatusRic.Return_SARM_To_BR
                        or StatusRic.Return_ECS_To_BR
                    )
                )
                    return BadRequest($"BR cannot forward from status {ric.Status}");

                ric.Status = StatusRic.Review_SARM;
                break;

            case Role.SARM_Pic:
                if (ric.Status != StatusRic.Review_SARM)
                    return BadRequest($"SARM cannot forward from status {ric.Status}");

                ric.Status = StatusRic.Review_ECS;
                break;

            case Role.ECS_Pic:
                if (ric.Status != StatusRic.Review_ECS)
                    return BadRequest($"ECS cannot forward from status {ric.Status}");

                ric.Status = StatusRic.Approval_Manager_User;
                await _repository.EnsureApprovalsCreatedAsync(ric.Id);
                break;

            default:
                return Forbid("Role not allowed.");
        }

        var ok = await _repository.MoveRicToNextStageAsync(ric, editorId);
        return ok ? Ok("RIC forwarded successfully.") : StatusCode(500, "Failed to forward RIC.");
    }

    [RoleRequired(
        Role.User_Manager,
        Role.User_VP,
        Role.BR_Manager,
        Role.SARM_Manager,
        Role.SARM_VP,
        Role.ECS_Manager,
        Role.ECS_VP
    )]
    [HttpPut("{id:guid}/approve")]
    // [Authorize(Roles = "User_Manager,User_VP,BR_Manager,SARM_Manager,SARM_VP,ECS_Manager,ECS_VP")]
    public async Task<IActionResult> Approve(Guid id)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var editorId = GetGuidClaim("id");
        var roleStr = GetStringClaim("role");

        if (!Enum.TryParse(roleStr, out Role role))
            return Forbid("Invalid role.");

        var ric = await _repository.GetByIdAsync(id);
        if (ric is null)
            return NotFound("RIC not found.");

        // role -> (approvalRole, requiredStatus, nextStatus)
        var approveMap = new Dictionary<
            Role,
            (RoleApproval ApprovalRole, StatusRic Required, StatusRic Next)
        >
        {
            [Role.User_Manager] = (
                RoleApproval.User_Manager,
                StatusRic.Approval_Manager_User,
                StatusRic.Approval_VP_User
            ),
            [Role.User_VP] = (
                RoleApproval.User_VP,
                StatusRic.Approval_VP_User,
                StatusRic.Approval_Manager_BR
            ),
            [Role.BR_Manager] = (
                RoleApproval.BR_Manager,
                StatusRic.Approval_Manager_BR,
                StatusRic.Approval_Manager_SARM
            ),
            [Role.SARM_Manager] = (
                RoleApproval.SARM_Manager,
                StatusRic.Approval_Manager_SARM,
                StatusRic.Approval_VP_SARM
            ),
            [Role.SARM_VP] = (
                RoleApproval.SARM_VP,
                StatusRic.Approval_VP_SARM,
                StatusRic.Approval_Manager_ECS
            ),
            [Role.ECS_Manager] = (
                RoleApproval.ECS_Manager,
                StatusRic.Approval_Manager_ECS,
                StatusRic.Approval_VP_ECS
            ),
            [Role.ECS_VP] = (RoleApproval.ECS_VP, StatusRic.Approval_VP_ECS, StatusRic.Done),
        };

        if (!approveMap.TryGetValue(role, out var cfg))
            return Forbid("Role is not allowed to approve.");

        if (ric.Status != cfg.Required)
            return BadRequest($"{role} cannot approve from status {ric.Status}.");

        // Ensure approval records exist (backfill for older data)
        await _repository.EnsureApprovalsCreatedAsync(id);

        // 1) mark approval pending -> approved
        var approvalOk = await _repository.MarkApprovalApprovedAsync(
            id,
            cfg.ApprovalRole,
            editorId
        );
        if (!approvalOk)
            return BadRequest($"No pending approval found for role {cfg.ApprovalRole}.");

        // 2) update RIC status
        ric.Status = cfg.Next;
        Touch(ric);

        // If you want: keep it pure via repository (recommended)
        // var ok = await _repository.UpdateAsync(ric);
        // if (!ok) return StatusCode(500, "Failed to approve RIC.");

        // current style kamu: attach + partial update
        _context.FormRics.Attach(ric);
        _context.Entry(ric).Property(x => x.Status).IsModified = true;
        _context.Entry(ric).Property(x => x.UpdatedAt).IsModified = true;
        await _context.SaveChangesAsync();

        return Ok(
            new
            {
                message = "RIC approved successfully.",
                id = ric.Id,
                status = ric.Status.ToString(),
            }
        );
    }

    [RoleRequired(Role.User_Pic)]
    [HttpDelete("{id:guid}")]
    // [Authorize(Roles = "User_Pic,BR_Pic,SARM_Pic")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _repository.DeleteAsync(id);
        return ok ? NoContent() : NotFound("RIC not found.");
    }

    [RoleRequired(
        Role.User_Manager,
        Role.User_VP,
        Role.BR_Manager,
        Role.SARM_Manager,
        Role.SARM_VP,
        Role.ECS_Manager,
        Role.ECS_VP
    )]
    [HttpGet("approval")]
    public async Task<IActionResult> GetApprovalQueue()
    {
        var groupId = GetGuidClaim("groupId");
        var roleStr = GetStringClaim("role");

        if (!Enum.TryParse(roleStr, out Role role))
            return Forbid("Invalid role.");

        var data = await _repository.GetApprovalQueueAsync(groupId, roleStr);
        return Ok(data);
    }
}
