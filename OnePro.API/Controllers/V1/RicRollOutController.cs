using System.Security.Claims;
using Core.Models;
using Core.Models.Entities;
using Core.Models.Enums;
using Core.RequestModels.RicRollOut;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnePro.API.Interfaces;

namespace OnePro.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RicRollOutController : ControllerBase
{
    private readonly IRicRollOutRepository _repository;
    private readonly OneProDbContext _context;

    public RicRollOutController(IRicRollOutRepository repository, OneProDbContext context)
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

    private bool TryGetGuid(string key, out Guid result)
    {
        result = Guid.Empty;
        var value = User.FindFirstValue(key);
        return Guid.TryParse(value, out result) && result != Guid.Empty;
    }

    private string GetString(string key)
    {
        // 1) exact match
        var value = User.FindFirstValue(key);

        // 2) fallback khusus role
        if (
            string.IsNullOrWhiteSpace(value)
            && key.Equals("role", StringComparison.OrdinalIgnoreCase)
        )
            value = User.FindFirstValue(ClaimTypes.Role);

        // 3) fallback name (opsional)
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
    // GET MY (By Group)
    // =========================
    [HttpGet("my")]
    public async Task<IActionResult> GetMyGroupRollOuts()
    {
        if (!TryGetGuid("groupId", out var groupId))
            return BadRequest("User does not belong to any group.");
        return Ok(await _repository.GetAllByGroupAsync(groupId));
    }

    // =========================
    // GET BY ID (raw entity)
    // =========================
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var rollOut = await _repository.GetByIdAsync(id);
        return rollOut is null ? NotFound("RIC Roll Out not found.") : Ok(rollOut);
    }

    [HttpGet("{id:guid}/detail/ric")]
    public async Task<IActionResult> GetDetailAsync(Guid id)
    {
        var detail = await _repository.GetDetailAsync(id);
        return detail is null ? NotFound("RIC Roll Out not found.") : Ok(detail);
    }

    // =========================
    // GET DETAIL
    // =========================
    [HttpGet("{id:guid}/detail")]
    public async Task<IActionResult> GetDetailById(Guid id)
    {
        var rollOut = await _repository.GetDetailByIdAsync(id);
        return rollOut is null ? NotFound("RIC Roll Out not found.") : Ok(rollOut);
    }

    // =========================
    // CREATE (User only)
    // User bisa create dengan Status: Draft atau Submitted_To_BR
    // =========================
    [HttpPost]
    [Authorize(Roles = "User_Pic")]
    public async Task<IActionResult> Create(CreateRicRollOutRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetGuid("id");
        if (!TryGetGuid("groupId", out var groupId))
            return BadRequest("User does not belong to any group.");

        // default Draft kalau client ngirim kosong/invalid
        var action = (req.Action ?? "save").Trim().ToLowerInvariant();

        var status = action switch
        {
            "submit" => StatusRicRollOut.Submitted_To_BR,
            "save" => StatusRicRollOut.Draft,
            _ => StatusRicRollOut.Draft,
        };

        var rollOut = new FormRicRollOut
        {
            IdUser = userId,
            IdGroupUser = groupId,

            Entitas = req.Entitas,
            JudulAplikasi = req.JudulAplikasi,
            Hashtag = req.Hashtag,

            CompareWithAsIsHoldingProcessFiles = req.CompareWithAsIsHoldingProcessFiles,
            StkAsIsToBeFiles = req.StkAsIsToBeFiles,

            IsJoinedDomainAdPertamina = req.IsJoinedDomainAdPertamina,
            IsUsingErpPertamina = req.IsUsingErpPertamina,
            IsImplementedRequiredActivation = req.IsImplementedRequiredActivation,
            HasDataCenterConnection = req.HasDataCenterConnection,
            HasRequiredResource = req.HasRequiredResource,

            Status = status,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        return await _repository.CreateAsync(rollOut)
            ? CreatedAtAction(nameof(GetById), new { id = rollOut.Id }, rollOut)
            : StatusCode(500, "Failed to create RIC Roll Out.");
    }

    // =========================
    // UPDATE (User only)
    // Allowed: Draft / Rejected_By_BR
    // =========================
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "User_Pic")]
    public async Task<IActionResult> Update(Guid id, UpdateRicRollOutRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!TryGetGuid("groupId", out var groupId))
            return BadRequest("User does not belong to any group.");
        var editorId = GetGuid("id");

        var rollOut = await _repository.GetByIdAsync(id);
        if (rollOut is null)
            return NotFound("RIC Roll Out not found.");

        if (rollOut.IdGroupUser != groupId)
            return Forbid();

        if (rollOut.Status is not (StatusRicRollOut.Draft or StatusRicRollOut.Rejected_By_BR))
            return BadRequest($"Roll Out cannot be updated from status {rollOut.Status}");

        // Merge/overwrite fields (pastikan file lama tidak hilang)
        rollOut.Entitas = req.Entitas;
        rollOut.JudulAplikasi = req.JudulAplikasi;
        rollOut.Hashtag = req.Hashtag;

        rollOut.CompareWithAsIsHoldingProcessFiles = req.CompareWithAsIsHoldingProcessFiles;
        rollOut.StkAsIsToBeFiles = req.StkAsIsToBeFiles;

        rollOut.IsJoinedDomainAdPertamina = req.IsJoinedDomainAdPertamina;
        rollOut.IsUsingErpPertamina = req.IsUsingErpPertamina;
        rollOut.IsImplementedRequiredActivation = req.IsImplementedRequiredActivation;
        rollOut.HasDataCenterConnection = req.HasDataCenterConnection;
        rollOut.HasRequiredResource = req.HasRequiredResource;

        rollOut.UpdatedAt = DateTime.UtcNow;

        // Distinguish save vs submit
        var action = (req.Action ?? "save").Trim().ToLowerInvariant();

        bool ok;
        if (action == "save")
        {
            // cuma update data, jangan pindah stage
            ok = await _repository.UpdateAsync(rollOut); // pastikan repo punya UpdateAsync
            return ok ? NoContent() : StatusCode(500, "Failed to save draft.");
        }

        if (action == "submit")
        {
            // submit dari draft / resubmit dari rejected
            if (rollOut.Status == StatusRicRollOut.Rejected_By_BR)
            {
                rollOut.Status = StatusRicRollOut.Review_BR;
                ok = await _repository.ResubmitAfterRejectionAsync(rollOut, editorId);
            }
            else
            {
                rollOut.Status = StatusRicRollOut.Submitted_To_BR;
                ok = await _repository.MoveRollOutToNextStageAsync(rollOut, editorId);
            }

            return ok
                ? Ok(new { message = "Submitted", status = rollOut.Status.ToString() })
                : StatusCode(500, "Failed to submit roll out.");
        }

        return BadRequest("Invalid action. Use save or submit.");
    }

    // =========================
    // Resubmit (User only)
    // Allowed: Reject by BR
    // =========================
    [HttpPut("{id:guid}/resubmit")]
    [Authorize(Roles = "User_Pic")]
    public async Task<IActionResult> ResubmitAfterRejection(Guid id, UpdateRicRollOutRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!TryGetGuid("groupId", out var groupId))
            return BadRequest("User does not belong to any group.");
        var editorId = GetGuid("id");

        var rollOut = await _repository.GetByIdAsync(id);
        if (rollOut is null)
            return NotFound("RIC Roll Out not found.");

        if (rollOut.IdGroupUser != groupId)
            return Forbid();

        // Must be rejected first
        if (rollOut.Status != StatusRicRollOut.Rejected_By_BR)
            return BadRequest($"Cannot resubmit from status {rollOut.Status}");

        // Overwrite fields
        rollOut.Entitas = req.Entitas;
        rollOut.JudulAplikasi = req.JudulAplikasi;
        rollOut.Hashtag = req.Hashtag;

        rollOut.CompareWithAsIsHoldingProcessFiles = req.CompareWithAsIsHoldingProcessFiles;
        rollOut.StkAsIsToBeFiles = req.StkAsIsToBeFiles;

        rollOut.IsJoinedDomainAdPertamina = req.IsJoinedDomainAdPertamina;
        rollOut.IsUsingErpPertamina = req.IsUsingErpPertamina;
        rollOut.IsImplementedRequiredActivation = req.IsImplementedRequiredActivation;
        rollOut.HasDataCenterConnection = req.HasDataCenterConnection;
        rollOut.HasRequiredResource = req.HasRequiredResource;

        // Move back to review stage
        rollOut.Status = StatusRicRollOut.Review_BR;
        rollOut.UpdatedAt = DateTime.UtcNow;

        var ok = await _repository.ResubmitAfterRejectionAsync(rollOut, editorId);

        return ok ? NoContent() : StatusCode(500, "Failed to resubmit RollOut.");
    }

    // =========================
    // DELETE (User only)
    // Allowed: Draft
    // =========================
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "User_Pic")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryGetGuid("groupId", out var groupId))
            return BadRequest("User does not belong to any group.");

        var rollOut = await _repository.GetByIdAsync(id);
        if (rollOut is null)
            return NotFound("RIC Roll Out not found.");

        if (rollOut.IdGroupUser != groupId)
            return Forbid();

        if (rollOut.Status != StatusRicRollOut.Draft)
            return BadRequest("Only Draft roll out can be deleted.");

        return await _repository.DeleteAsync(id)
            ? NoContent()
            : StatusCode(500, "Failed to delete.");
    }

    // =========================
    // SUBMIT (User only)
    // Draft / Rejected -> Submitted_To_BR
    // =========================
    [HttpPut("{id:guid}/submit")]
    [Authorize(Roles = "User_Pic")]
    public async Task<IActionResult> Submit(Guid id)
    {
        if (!TryGetGuid("groupId", out var groupId))
            return BadRequest("User does not belong to any group.");
        var editorId = GetGuid("id");

        var rollOut = await _repository.GetByIdAsync(id);
        if (rollOut is null)
            return NotFound("RIC Roll Out not found.");

        if (rollOut.IdGroupUser != groupId)
            return Forbid();

        if (rollOut.Status is not (StatusRicRollOut.Draft or StatusRicRollOut.Rejected_By_BR))
            return BadRequest($"Cannot submit from status {rollOut.Status}");

        rollOut.Status = StatusRicRollOut.Submitted_To_BR;
        rollOut.UpdatedAt = DateTime.UtcNow;

        return await _repository.MoveRollOutToNextStageAsync(rollOut, editorId)
            ? Ok(new { message = "Submitted to BR", status = rollOut.Status.ToString() })
            : StatusCode(500, "Failed to submit RIC Roll Out.");
    }

    // =========================
    // BR START REVIEW
    // Submitted_To_BR -> Review_BR
    // =========================
    [HttpPut("{id:guid}/review")]
    [Authorize(Roles = "BR_Pic")]
    public async Task<IActionResult> StartReview(Guid id)
    {
        var editorId = GetGuid("id");
        var roleStr = GetString("role");

        if (!Enum.TryParse<Role>(roleStr, out var role) || role != Role.BR_Pic)
            return Forbid("Invalid role.");

        var rollOut = await _repository.GetByIdAsync(id);
        if (rollOut is null)
            return NotFound("RIC Roll Out not found.");

        if (rollOut.Status != StatusRicRollOut.Submitted_To_BR)
            return BadRequest($"BR cannot review from status {rollOut.Status}");

        rollOut.Status = StatusRicRollOut.Review_BR;
        rollOut.UpdatedAt = DateTime.UtcNow;

        return await _repository.MoveRollOutToNextStageAsync(rollOut, editorId)
            ? Ok(new { message = "BR review started", status = rollOut.Status.ToString() })
            : StatusCode(500, "Failed to start review.");
    }

    // =========================
    // BR REJECT + NOTE
    // Review_BR -> Rejected_By_BR
    // =========================
    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "BR_Pic")]
    public async Task<IActionResult> Reject(Guid id, RejectRicRollOutRequest req)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetGuid("id");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return Unauthorized("User not found.");

        if (user.Role != Role.BR_Pic)
            return Forbid();

        var rollOut = await _repository.GetByIdAsync(id);
        if (rollOut is null)
            return NotFound("RIC Roll Out not found.");

        if (
            rollOut.Status != StatusRicRollOut.Review_BR
            && rollOut.Status != StatusRicRollOut.Submitted_To_BR
        )
            return BadRequest($"Cannot reject from status {rollOut.Status}");

        rollOut.Status = StatusRicRollOut.Rejected_By_BR;
        rollOut.UpdatedAt = DateTime.UtcNow;

        await _repository.AddReviewAsync(
            new ReviewFormRicRollOut
            {
                IdFormRicRollOut = rollOut.Id,
                IdUser = userId,
                Catatan = req.Catatan,
                RoleReview = RoleReview.BR,
                CreatedAt = DateTime.UtcNow,
            }
        );

        await _repository.MoveRollOutToNextStageAsync(rollOut, userId);

        return Ok(
            new
            {
                message = "Roll Out rejected",
                status = rollOut.Status.ToString(),
                reviewer = "BR",
            }
        );
    }

    // =========================
    // BR FORWARD to BR MANAGER
    // Review_BR -> Submitted_To_BR_Manager
    // =========================
    [HttpPut("{id:guid}/forward")]
    [Authorize(Roles = "BR_Pic")]
    public async Task<IActionResult> Forward(Guid id)
    {
        var editorId = GetGuid("id");

        var rollOut = await _repository.GetByIdAsync(id);
        if (rollOut is null)
            return NotFound("RIC Roll Out not found.");

        if (rollOut.Status != StatusRicRollOut.Review_BR)
            return BadRequest($"BR cannot forward from status {rollOut.Status}");

        // bikin approval record untuk BR_Manager (idempotent)
        await _repository.EnsureApprovalsCreatedAsync(rollOut.Id);

        rollOut.Status = StatusRicRollOut.Submitted_To_BR_Manager;
        rollOut.UpdatedAt = DateTime.UtcNow;

        return await _repository.MoveRollOutToNextStageAsync(rollOut, editorId)
            ? Ok(new { message = "Forwarded to BR Manager", status = rollOut.Status.ToString() })
            : StatusCode(500, "Failed to forward.");
    }

    // =========================
    // BR MANAGER APPROVE
    // Submitted_To_BR_Manager -> Approved_By_BR_Manager
    // =========================
    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "BR_Manager")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var editorId = GetGuid("id");
        var roleStr = GetString("role");

        if (!Enum.TryParse<Role>(roleStr, out var role) || role != Role.BR_Manager)
            return Forbid("Invalid role.");

        var rollOut = await _repository.GetByIdAsync(id);
        if (rollOut is null)
            return NotFound("RIC Roll Out not found.");

        if (rollOut.Status != StatusRicRollOut.Submitted_To_BR_Manager)
            return BadRequest($"BR Manager cannot approve from status {rollOut.Status}");

        // update approval record
        var approvalOk = await _repository.MarkApprovalApprovedAsync(
            id,
            RoleApproval.BR_Manager,
            editorId
        );

        if (!approvalOk)
            return BadRequest("No pending approval found for BR_Manager.");

        rollOut.Status = StatusRicRollOut.Approved_By_BR_Manager;
        rollOut.UpdatedAt = DateTime.UtcNow;

        // attach minimal update (biar hemat query tracking)
        _context.FormRicRollOuts.Attach(rollOut);
        _context.Entry(rollOut).Property(x => x.Status).IsModified = true;
        _context.Entry(rollOut).Property(x => x.UpdatedAt).IsModified = true;
        await _context.SaveChangesAsync();

        return Ok(
            new
            {
                message = "Roll Out approved successfully.",
                id = rollOut.Id,
                status = rollOut.Status.ToString(),
            }
        );
    }
}
