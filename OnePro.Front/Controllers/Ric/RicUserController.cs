using Core.Models.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnePro.Front.Mappers;
using OnePro.Front.Middleware;
using OnePro.Front.Models;
using OnePro.Front.Services.Interfaces;

namespace OnePro.Front.Controllers.Ric
{
    public class RicUserController : RicControllerBase
    {
        private const string ViewUserIndex = "~/Views/Ric/User/Index.cshtml";
        private const string ViewUserCreate = "~/Views/Ric/User/Create.cshtml";
        private const string ViewUserEdit = "~/Views/Ric/User/Edit.cshtml";
        private const string ViewUserUpdate = "~/Views/Ric/User/Update.cshtml";

        private const string ViewUserApprovalIndex = "~/Views/Ric/Approval/Index.cshtml";
        private const string ViewUserApprovalDetail = "~/Views/Ric/Approval/Detail.cshtml";

        public RicUserController(
            IRicService ricService,
            ILogger<RicUserController> logger,
            IWebHostEnvironment env
        )
            : base(ricService, logger, env) { }

        [RoleRequired(Role.User_Member, Role.User_Pic, Role.User_Manager, Role.User_VP)]
        [HttpGet("Ric/User")]
        public async Task<IActionResult> UserIndex(string? q, string? status)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var rics = await RicService.GetMyRicsAsync(token, q, 50);
            if (!string.IsNullOrWhiteSpace(status))
            {
                rics = rics.Where(x =>
                        string.Equals(x.Status, status, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();
            }

            rics = rics.Take(10).ToList();
            return View(ViewUserIndex, rics);
        }

        [HttpGet("Ric/User/Create")]
        public IActionResult Create()
        {
            if (!TryGetToken(out _))
                return RedirectToLogin();
            return View(ViewUserCreate, new RicCreateViewModel());
        }

        [HttpPost("Ric/User/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RicCreateViewModel model, string action)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();
            if (!ModelState.IsValid)
                return View(ViewUserCreate, model);

            try
            {
                var dto = await RicMapper.MapToCreateRequestAsync(model, action, SaveFilesAsync);
                await RicService.CreateRicAsync(dto, token);

                TempData["SuccessMessage"] = "RIC berhasil dibuat!";
                return RedirectToAction(nameof(UserIndex));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating RIC");
                ModelState.AddModelError(string.Empty, "Terjadi kesalahan saat membuat RIC.");
                return View(ViewUserCreate, model);
            }
        }

        [HttpGet("Ric/User/Edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var ric = await RicService.GetRicByIdAsync(id, token);
            if (ric == null)
                return NotFound();

            if ((StatusRic)ric.Status != StatusRic.Draft)
                return RejectByStatus(
                    "RIC hanya bisa diedit kalau status masih Draft.",
                    nameof(UserIndex)
                );

            var vm = RicMapper.MapToEditViewModel(ric);
            ModelState.Clear();
            return View(ViewUserEdit, vm);
        }

        [HttpPost("Ric/User/Delete/{id:guid}")]
        [ValidateAntiForgeryToken]
        [RoleRequired(Role.User_Pic, Role.User_Manager, Role.User_VP)]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var ric = await RicService.GetRicByIdAsync(id, token);
            if (ric == null)
                return NotFound();

            // Guard: cuma boleh hapus kalau Draft
            if ((StatusRic)ric.Status != StatusRic.Draft)
                return RejectByStatus(
                    "RIC hanya bisa dihapus jika status masih Draft.",
                    nameof(UserIndex)
                );

            DeleteFilesFromWebRoot(ric.AsIsProcessRasciFile);
            DeleteFilesFromWebRoot(ric.ToBeProcessBusinessRasciKkiFile);
            DeleteFilesFromWebRoot(ric.ExcpectedCompletionTargetFile);

            var success = await RicService.DeleteRicAsync(id, token);
            if (!success)
            {
                TempData["ErrorMessage"] = "Gagal menghapus RIC.";
                return RedirectToAction(nameof(UserIndex));
            }

            TempData["SuccessMessage"] = "RIC & seluruh file berhasil dihapus.";
            return RedirectToAction(nameof(UserIndex));
        }

        [HttpPost("Ric/User/Edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, RicCreateViewModel model, string action)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();
            if (!ModelState.IsValid)
                return View(ViewUserEdit, model);

            var existing = await RicService.GetRicByIdAsync(id, token);
            if (existing == null)
                return NotFound();

            if ((StatusRic)existing.Status != StatusRic.Draft)
                return RejectByStatus(
                    "RIC hanya bisa diedit kalau status masih Draft.",
                    nameof(UserIndex)
                );

            try
            {
                var dto = await RicMapper.MapToUpdateRequestAsync(
                    id,
                    model,
                    action,
                    existing,
                    SaveFilesAsync
                );

                // Update dulu agar aman
                await RicService.UpdateRicAsync(id, dto, token);

                // Hapus file lama jika berubah
                if (!SameFiles(existing.AsIsProcessRasciFile, dto.AsIsProcessRasciFile))
                    DeleteFilesFromWebRoot(existing.AsIsProcessRasciFile);

                if (
                    !SameFiles(
                        existing.ToBeProcessBusinessRasciKkiFile,
                        dto.ToBeProcessBusinessRasciKkiFile
                    )
                )
                    DeleteFilesFromWebRoot(existing.ToBeProcessBusinessRasciKkiFile);

                if (
                    !SameFiles(
                        existing.ExcpectedCompletionTargetFile,
                        dto.ExcpectedCompletionTargetFile
                    )
                )
                    DeleteFilesFromWebRoot(existing.ExcpectedCompletionTargetFile);

                TempData["SuccessMessage"] = "RIC berhasil diperbarui!";
                return RedirectToAction(nameof(UserIndex));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating RIC {Id}", id);
                ModelState.AddModelError(string.Empty, "Terjadi kesalahan saat update RIC.");
                return View(ViewUserEdit, model);
            }
        }

        [HttpGet("Ric/User/Update/{id:guid}")]
        public async Task<IActionResult> Update(Guid id)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var ric = await RicService.GetRicByIdAsync(id, token);
            if (ric == null)
                return NotFound();

            if ((StatusRic)ric.Status != StatusRic.Return_BR_To_User)
                return RejectByStatus(
                    "RIC hanya bisa diupdate kalau status masih Return_BR_To_User.",
                    nameof(UserIndex)
                );

            var vm = RicMapper.MapToEditViewModel(ric);
            ModelState.Clear();
            return View(ViewUserUpdate, vm);
        }

        [HttpPost("Ric/User/Update/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Guid id, RicCreateViewModel model, string action)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();
            if (!ModelState.IsValid)
                return View(ViewUserUpdate, model);

            var existing = await RicService.GetRicByIdAsync(id, token);
            if (existing == null)
                return NotFound();

            if ((StatusRic)existing.Status != StatusRic.Return_BR_To_User)
                return RejectByStatus(
                    "RIC hanya bisa diupdate kalau status masih Return_BR_To_User.",
                    nameof(UserIndex)
                );

            try
            {
                var dto = await RicMapper.MapToResubmitRequestAsync(
                    model,
                    action,
                    existing,
                    SaveFilesAsync
                );
                await RicService.ResubmitRicAsync(id, dto, token);

                TempData["SuccessMessage"] = "RIC berhasil di-resubmit!";
                return RedirectToAction(nameof(UserIndex));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error resubmitting RIC {Id}", id);
                ModelState.AddModelError(string.Empty, "Terjadi kesalahan saat resubmit RIC.");
                return View(ViewUserUpdate, model);
            }
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
        [HttpGet("Ric/Approval")]
        public async Task<IActionResult> ApprovalIndex(string? q, string? status)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var approvalRics = await RicService.GetApprovalQueueAsync(token, q, 50);
            if (!string.IsNullOrWhiteSpace(status))
            {
                approvalRics = approvalRics
                    .Where(x => string.Equals(x.Status, status, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            approvalRics = approvalRics.Take(10).ToList();
            return View(ViewUserApprovalIndex, approvalRics);
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
        [HttpGet("Ric/Approval/{id:guid}")]
        public async Task<IActionResult> Approval(Guid id)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var ric = await RicService.GetRicByIdAsync(id, token);
            if (ric == null)
                return NotFound();

            var vm = RicMapper.MapToEditViewModel(ric);

            return View(ViewUserApprovalDetail, vm);
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
        [HttpPost("~/Ric/Approval/{id:guid}/approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAction(Guid id)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var ok = await RicService.ApproveAsync(id, token);
            if (!ok)
            {
                TempData["ErrorMessage"] =
                    "Gagal approve RIC. Cek status/role atau pending approval belum ada.";
                return RedirectToAction(nameof(Approval), new { id });
            }

            TempData["SuccessMessage"] = "RIC berhasil di-approve.";
            return RedirectToAction(nameof(Approval), new { id });
        }

        // Helper
        private void DeleteFilesFromWebRoot(IEnumerable<string>? urls)
        {
            if (urls == null)
                return;

            foreach (var url in urls.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                try
                {
                    // contoh url: "/uploads/ric/a.pdf"
                    var relative = url.Trim()
                        .TrimStart('~')
                        .TrimStart('/')
                        .Replace('/', Path.DirectorySeparatorChar);

                    var fullPath = Path.GetFullPath(Path.Combine(Env.WebRootPath, relative));
                    var webRootFull = Path.GetFullPath(Env.WebRootPath);

                    // safety: pastiin masih di dalam wwwroot
                    if (!fullPath.StartsWith(webRootFull, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed deleting file url={Url}", url);
                    // jangan throw --- biar proses delete RIC tetap lanjut
                }
            }
        }

        private static bool SameFiles(IEnumerable<string>? a, IEnumerable<string>? b)
        {
            static string N(string s) => (s ?? "").Trim().ToLowerInvariant();

            var la = (a ?? Enumerable.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(N)
                .OrderBy(x => x)
                .ToList();
            var lb = (b ?? Enumerable.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(N)
                .OrderBy(x => x)
                .ToList();

            if (la.Count != lb.Count)
                return false;
            return la.SequenceEqual(lb);
        }
    }
}
