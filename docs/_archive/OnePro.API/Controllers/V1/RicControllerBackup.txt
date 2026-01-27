using System.Security.Claims;
using System.Text.Json;
using Core.Models;
using Core.Models.Entities;
using Core.Models.Enums;
using Core.Models.Enums;
using Core.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private Guid GetGuid(string key)
    {
        var value = User.FindFirstValue(key);
        if (!Guid.TryParse(value, out var result))
            throw new InvalidOperationException($"{key} missing in token");
        return result;
    }

    private string GetString(string key)
    {
        // 1) exact match (mis. "id", "groupId", "role")
        var value = User.FindFirstValue(key);

        // 2) fallback khusus untuk "role" (karena sering dimap ke ClaimTypes.Role)
        if (
            string.IsNullOrWhiteSpace(value)
            && key.Equals("role", StringComparison.OrdinalIgnoreCase)
        )
            value = User.FindFirstValue(ClaimTypes.Role);

        // 3) fallback kalau lo suatu saat pakai ClaimTypes.Name (opsional)
        if (
            string.IsNullOrWhiteSpace(value)
            && key.Equals("name", StringComparison.OrdinalIgnoreCase)
        )
            value = User.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{key} missing in token");

        return value;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyGroupRics()
    {
        var groupId = GetGuid("groupId");
        return Ok(await _repository.GetAllByGroupAsync(groupId));
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

    [HttpPost]
    [Authorize(Roles = "User_Pic,BR_Pic,SARM_Pic")]
    public async Task<IActionResult> Create(FormRicRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ric = new FormRic
        {
            IdUser = GetGuid("id"),
            IdGroupUser = GetGuid("groupId"),

            Judul = req.Judul,
            Hastag = req.Hastag,
            AsIsProcessRasciFile = req.AsIsProcessRasciFile,
            Permasalahan = req.Permasalahan,
            DampakMasalah = req.DampakMasalah,
            FaktorPenyebabMasalah = req.FaktorPenyebabMasalah,
            SolusiSaatIni = req.SolusiSaatIni,
            AlternatifSolusi = req.AlternatifSolusi,
            ToBeProcessBusinessRasciKkiFile = req.ToBeProcessBusinessRasciKkiFile,
            PotensiValueCreation = req.PotensiValueCreation,
            ExcpectedCompletionTargetFile = req.ExcpectedCompletionTargetFile,
            HasilSetelahPerbaikan = req.HasilSetelahPerbaikan,

            Status = req.Status,
            BrConfirm = false,
            SarmConfirm = false,
            EcsConfirm = false,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        return await _repository.CreateAsync(ric)
            ? CreatedAtAction(nameof(GetById), new { id = ric.Id }, ric)
            : StatusCode(500, "Failed to create RIC.");
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "User_Pic,BR_Pic,SARM_Pic")]
    public async Task<IActionResult> Update(Guid id, FormRicRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var groupId = GetGuid("groupId");
        var ric = await _repository.GetByIdAsync(id);

        if (ric is null)
            return NotFound("RIC not found.");

        if (ric.IdGroupUser != groupId)
            return Forbid();

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
        ric.Status = req.Status;
        ric.UpdatedAt = DateTime.UtcNow;

        return await _repository.UpdateAsync(ric)
            ? NoContent()
            : StatusCode(500, "Failed to update RIC.");
    }

    [HttpPut("{id:guid}/resubmit")]
    [Authorize(Roles = "User_Pic,BR_Pic,SARM_Pic")]
    public async Task<IActionResult> ResubmitAfterRejection(Guid id, FormRicRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var groupId = GetGuid("groupId");
        var editorId = GetGuid("id");

        var ric = await _repository.GetByIdAsync(id);

        if (ric is null)
            return NotFound("RIC not found.");

        if (ric.IdGroupUser != groupId)
            return Forbid();

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
        ric.Status = StatusRic.Review_BR;
        ric.UpdatedAt = DateTime.UtcNow;

        return await _repository.ResubmitAfterRejection(ric, editorId)
            ? NoContent()
            : StatusCode(500, "Failed to update RIC.");
    }

    [HttpPut("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, RejectRicRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetGuid("id");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return Unauthorized("User not found.");

        RoleReview reviewRole;
        StatusRic newStatus;

        switch (user.Role)
        {
            case Role.BR_Pic:
                reviewRole = RoleReview.BR;
                newStatus = StatusRic.Return_BR_To_User;
                break;

            case Role.ECS_Pic:
                reviewRole = RoleReview.ECS;
                newStatus = StatusRic.Return_ECS_To_BR;
                break;

            case Role.SARM_Pic:
                reviewRole = RoleReview.SARM;
                newStatus = StatusRic.Return_SARM_To_BR;
                break;

            default:
                return Forbid();
        }

        var ric = await _repository.GetByIdAsync(id);
        if (ric is null)
            return NotFound("RIC not found.");

        ric.Status = newStatus;
        ric.UpdatedAt = DateTime.UtcNow;

        await _repository.AddReviewAsync(
            new ReviewFormRic
            {
                IdFormRic = ric.Id,
                IdUser = userId,
                Catatan = req.Catatan,
                RoleReview = reviewRole,
                CreatedAt = DateTime.UtcNow,
            }
        );

        await _repository.UpdateAsync(ric);

        return Ok(
            new
            {
                message = "RIC rejected",
                status = ric.Status.ToString(),
                reviewer = reviewRole.ToString(),
            }
        );
    }

    // [HttpPut("{id:guid}/forward")]
    // [Authorize(Roles = "User_Pic")]
    // public async Task<IActionResult> Forward(Guid id, FormRicRequest req)
    // {
    //     if (!ModelState.IsValid)
    //         return ValidationProblem(ModelState);

    //     var editorId = GetGuid("id");
    //     var groupId = GetGuid("groupId");
    //     var role = GetString("role");

    //     Enum.TryParse<Role>(roleStr, out var role)

    //     if (!Enum.TryParse<Role>(roleStr, out var role))
    //         return Forbid("Invalid role.");

    //     var ric = await _repository.GetByIdAsync(id);
    //     if (ric is null)
    //         return NotFound("RIC not found.");

    //     if (ric.IdGroupUser != groupId)
    //         return Forbid();

    //     if (ric.Status != StatusRic.Return_BR_To_User)
    //         return BadRequest("RIC is not in returned state.");

    //     // await _repository.AddHistoryAsync(
    //     //     new FormRicHistory
    //     //     {
    //     //         IdFormRic = ric.Id,
    //     //         IdEditor = userId,
    //     //         Version = 1,
    //     //         SnapshotJson = JsonSerializer.Serialize(ric),
    //     //         EditedFieldsJson = "Resubmitted after reject",
    //     //         CreatedAt = DateTime.UtcNow,
    //     //     }
    //     // );

    //     ric.Judul = req.Judul;
    //     ric.Hastag = req.Hastag;
    //     ric.AsIsProcessRasciFile = req.AsIsProcessRasciFile;
    //     ric.Permasalahan = req.Permasalahan;
    //     ric.DampakMasalah = req.DampakMasalah;
    //     ric.FaktorPenyebabMasalah = req.FaktorPenyebabMasalah;
    //     ric.SolusiSaatIni = req.SolusiSaatIni;
    //     ric.AlternatifSolusi = req.AlternatifSolusi;
    //     ric.ToBeProcessBusinessRasciKkiFile = req.ToBeProcessBusinessRasciKkiFile;
    //     ric.PotensiValueCreation = req.PotensiValueCreation;
    //     ric.ExcpectedCompletionTargetFile = req.ExcpectedCompletionTargetFile;
    //     ric.HasilSetelahPerbaikan = req.HasilSetelahPerbaikan;

    //     // ric.Status = StatusRic.Review_SARM;
    //     ric.UpdatedAt = DateTime.UtcNow;

    //     if (role == Role.BR_Pic)
    //     {
    //         ric.Status = StatusRic.Review_SARM;
    //     }
    //     else if (role == Role.SARM_Pic)
    //     {
    //         ric.Status = StatusRic.Review_SARM;
    //     }
    //     else if (role == Role.ECS_Pic)
    //     {
    //         ric.Status = StatusRic.Approval_Manager_User;
    //     }

    //     return await _repository.UpdateAsync(ric)
    //         ? Ok("RIC resubmitted successfully.")
    //         : StatusCode(500, "Failed to submit RIC.");
    // }

    [HttpPut("{id:guid}/forward")]
    [Authorize(Roles = "BR_Pic,SARM_Pic,ECS_Pic")]
    // [Authorize(Roles = "BR_Pic,SARM_Pic")]
    public async Task<IActionResult> Forward(Guid id, FormRicRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var editorId = GetGuid("id");
        var groupId = GetGuid("groupId");
        var roleStr = GetString("role");

        if (editorId == Guid.Empty || string.IsNullOrEmpty(roleStr))
            return Unauthorized();

        if (!Enum.TryParse<Role>(roleStr, out var role))
            return Forbid("Invalid role.");

        var ric = await _repository.GetByIdAsync(id);
        if (ric is null)
            return NotFound("RIC not found.");

        // kalau rule lo memang harus satu group
        // if (ric.IdGroupUser != groupId)
        //     return Forbid();

        // Update fields (kalau forward memang membawa revisi data)
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

        ric.UpdatedAt = DateTime.UtcNow;

        // Role-based status transition + status guard
        if (role == Role.BR_Pic)
        {
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
        }
        else if (role == Role.SARM_Pic)
        {
            if (ric.Status != StatusRic.Review_SARM)
                return BadRequest("Status RIC tidak sesuai untuk SARM.");

            ric.Status = StatusRic.Review_ECS;
        }
        else if (role == Role.ECS_Pic)
        {
            if (ric.Status != StatusRic.Review_ECS)
                return BadRequest("Status RIC tidak sesuai untuk ECS.");

            ric.Status = StatusRic.Approval_Manager_User;

            await _repository.EnsureApprovalsCreatedAsync(ric.Id);
        }
        else
        {
            return Forbid("Role not allowed.");
        }

        return await _repository.MoveRicToNextStageAsync(ric, editorId)
            ? Ok("RIC forwarded successfully.")
            : StatusCode(500, "Failed to forward RIC.");
    }

    // [HttpPut("{id:guid}/approve")]
    // [Authorize(Roles = "User_Manager,User_VP,BR_Manager,SARM_Manager,SARM_VP,ECS_Manager,ECS_VP")]
    // public async Task<IActionResult> Approve(Guid id)
    // {
    //     if (!ModelState.IsValid)
    //         return ValidationProblem(ModelState);

    //     var editorId = GetGuid("id");
    //     var groupId = GetGuid("groupId");
    //     var roleStr = GetString("role");

    //     if (editorId == Guid.Empty || string.IsNullOrEmpty(roleStr))
    //         return Unauthorized();

    //     if (!Enum.TryParse<Role>(roleStr, out var role))
    //         return Forbid("Invalid role.");

    //     var ric = await _repository.GetByIdAsync(id);
    //     if (ric is null)
    //         return NotFound("RIC not found.");

    //     // Role-based status transition + status guard
    //     if (role == Role.User_Manager)
    //     {
    //         if (ric.Status is not (StatusRic.Approval_Manager_User))
    //             return BadRequest($"BR cannot forward from status {ric.Status}");

    //         ric.Status = StatusRic.Approval_VP_User;
    //     }
    //     else if (role == Role.User_VP)
    //     {
    //         if (ric.Status is not (StatusRic.Approval_VP_User))
    //             return BadRequest($"BR cannot forward from status {ric.Status}");

    //         ric.Status = StatusRic.Approval_Manager_BR;
    //     }
    //     else if (role == Role.BR_Manager)
    //     {
    //         if (ric.Status is not (StatusRic.Approval_Manager_BR))
    //             return BadRequest($"BR cannot forward from status {ric.Status}");

    //         ric.Status = StatusRic.Approval_Manager_SARM;
    //     }
    //     else if (role == Role.SARM_Manager)
    //     {
    //         if (ric.Status is not (StatusRic.Approval_Manager_SARM))
    //             return BadRequest($"BR cannot forward from status {ric.Status}");

    //         ric.Status = StatusRic.Approval_VP_SARM;
    //     }
    //     else if (role == Role.SARM_VP)
    //     {
    //         if (ric.Status is not (StatusRic.Approval_VP_SARM))
    //             return BadRequest($"BR cannot forward from status {ric.Status}");

    //         ric.Status = StatusRic.Approval_Manager_ECS;
    //     }
    //     else if (role == Role.ECS_Manager)
    //     {
    //         if (ric.Status is not (StatusRic.Approval_Manager_ECS))
    //             return BadRequest($"BR cannot forward from status {ric.Status}");

    //         ric.Status = StatusRic.Approval_VP_ECS;
    //     }
    //     else if (role == Role.ECS_VP)
    //     {
    //         if (ric.Status is not (StatusRic.Approval_VP_ECS))
    //             return BadRequest($"BR cannot forward from status {ric.Status}");

    //         ric.Status = StatusRic.Done;
    //     }

    //     ric.UpdatedAt = DateTime.UtcNow;
    //     // return Ok("Mantap gan");
    //     var approvalOk = await _repository.MarkApprovalApprovedAsync(
    //         id,
    //         approvalRole.Value,
    //         editorId
    //     );
    //     if (!approvalOk)
    //         return BadRequest($"No pending approval found for role {approvalRole.Value}.");
    //     // return await _repository.MoveRicToNextStageAsync(ric, editorId)
    //     //     ? Ok("RIC forwarded successfully.")
    //     //     : StatusCode(500, "Failed to forward RIC.");
    // }

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "User_Manager,User_VP,BR_Manager,SARM_Manager,SARM_VP,ECS_Manager,ECS_VP")]
    public async Task<IActionResult> Approve(Guid id)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var editorId = GetGuid("id");
        var roleStr = GetString("role");

        if (editorId == Guid.Empty || string.IsNullOrWhiteSpace(roleStr))
            return Unauthorized();

        if (!Enum.TryParse<Role>(roleStr, out var role))
            return Forbid("Invalid role.");

        var ric = await _repository.GetByIdAsync(id);
        if (ric is null)
            return NotFound("RIC not found.");

        // map Role -> RoleApproval (biar update tabel approvals)
        var approvalRole = role switch
        {
            Role.User_Manager => RoleApproval.User_Manager,
            Role.User_VP => RoleApproval.User_VP,
            Role.BR_Manager => RoleApproval.BR_Manager,
            Role.SARM_Manager => RoleApproval.SARM_Manager,
            Role.SARM_VP => RoleApproval.SARM_VP,
            Role.ECS_Manager => RoleApproval.ECS_Manager,
            Role.ECS_VP => RoleApproval.ECS_VP,
            _ => (RoleApproval?)null,
        };

        if (approvalRole is null)
            return Forbid("Role is not allowed to approve.");

        // guard + transition
        if (role == Role.User_Manager)
        {
            if (ric.Status != StatusRic.Approval_Manager_User)
                return BadRequest($"User Manager cannot approve from status {ric.Status}.");

            ric.Status = StatusRic.Approval_VP_User;
        }
        else if (role == Role.User_VP)
        {
            if (ric.Status != StatusRic.Approval_VP_User)
                return BadRequest($"User VP cannot approve from status {ric.Status}.");

            ric.Status = StatusRic.Approval_Manager_BR;
        }
        else if (role == Role.BR_Manager)
        {
            if (ric.Status != StatusRic.Approval_Manager_BR)
                return BadRequest($"BR Manager cannot approve from status {ric.Status}.");

            ric.Status = StatusRic.Approval_Manager_SARM;
        }
        else if (role == Role.SARM_Manager)
        {
            if (ric.Status != StatusRic.Approval_Manager_SARM)
                return BadRequest($"SARM Manager cannot approve from status {ric.Status}.");

            ric.Status = StatusRic.Approval_VP_SARM;
        }
        else if (role == Role.SARM_VP)
        {
            if (ric.Status != StatusRic.Approval_VP_SARM)
                return BadRequest($"SARM VP cannot approve from status {ric.Status}.");

            ric.Status = StatusRic.Approval_Manager_ECS;
        }
        else if (role == Role.ECS_Manager)
        {
            if (ric.Status != StatusRic.Approval_Manager_ECS)
                return BadRequest($"ECS Manager cannot approve from status {ric.Status}.");

            ric.Status = StatusRic.Approval_VP_ECS;
        }
        else if (role == Role.ECS_VP)
        {
            if (ric.Status != StatusRic.Approval_VP_ECS)
                return BadRequest($"ECS VP cannot approve from status {ric.Status}.");

            ric.Status = StatusRic.Done;
        }

        ric.UpdatedAt = DateTime.UtcNow;

        // 1) update approval record (Pending -> Approved)
        var approvalOk = await _repository.MarkApprovalApprovedAsync(
            id,
            approvalRole.Value,
            editorId
        );
        if (!approvalOk)
            return BadRequest($"No pending approval found for role {approvalRole.Value}.");

        _context.FormRics.Attach(ric);
        _context.Entry(ric).Property(x => x.Status).IsModified = true;
        _context.Entry(ric).Property(x => x.UpdatedAt).IsModified = true;

        await _context.SaveChangesAsync();

        // var ok = await _repository.MoveRicToNextStageAsync(ric, editorId);
        // if (!ok)
        //     return StatusCode(500, "Failed to approve RIC.");

        return Ok(
            new
            {
                message = "RIC approved successfully.",
                id = ric.Id,
                status = ric.Status.ToString(),
            }
        );
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "User_Pic,BR_Pic,SARM_Pic")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _repository.DeleteAsync(id) ? NoContent() : NotFound("RIC not found.");
    }
}
