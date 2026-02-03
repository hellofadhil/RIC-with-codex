using Core.Models.Enums;
using Core.RequestModels.RicRollOut;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnePro.Front.Middleware;
using OnePro.Front.Models;
using OnePro.Front.Models.RicRollOut;
using OnePro.Front.Services.Interfaces;

namespace OnePro.Front.Controllers.RicRollOut
{
    public class RicRollOutUserController : RicRollOutControllerBase
    {
        private const string ViewUserIndex = "~/Views/RicRollOut/User/Index.cshtml";
        private const string ViewUserCreate = "~/Views/RicRollOut/User/Create.cshtml";
        private const string ViewUserEdit = "~/Views/RicRollOut/User/Edit.cshtml";
        private const string ViewUserUpdate = "~/Views/RicRollOut/User/Update.cshtml";

        private const string ViewApprovalIndex = "~/Views/RicRollOut/Approval/Index.cshtml";
        private const string ViewApprovalDetail = "~/Views/RicRollOut/Approval/Detail.cshtml";

        public RicRollOutUserController(
            IRicRollOutService rollOutService,
            ILogger<RicRollOutUserController> logger,
            IWebHostEnvironment env
        )
            : base(rollOutService, logger, env) { }

        [RoleRequired(Role.User_Member, Role.User_Pic, Role.User_Manager, Role.User_VP)]
        [HttpGet("RicRollOut/User")]
        public async Task<IActionResult> UserIndex()
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var items = await RollOutService.GetMyRollOutsAsync(token);
            return View(ViewUserIndex, items);
        }

        [RoleRequired(Role.User_Member, Role.User_Pic, Role.User_Manager, Role.User_VP)]
        [HttpGet("RicRollOut/User/Files/{id:guid}/{kind}")]
        public async Task<IActionResult> DownloadFiles(Guid id, string kind)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var detail = await RollOutService.GetDetailByIdAsync(id, token);
            if (detail == null)
                return NotFound();

            var urls = kind?.ToLowerInvariant() switch
            {
                "compare" => detail.CompareWithAsIsHoldingProcessFiles,
                "stk" => detail.StkAsIsToBeFiles,
                _ => null
            };

            if (urls == null || urls.Count == 0)
                return NotFound("No files.");

            var zipName = $"RIC_RollOut_{id}_{kind}.zip";
            return DownloadZipFromUrls(urls, zipName);
        }

        [HttpGet("RicRollOut/User/Create")]
        public IActionResult Create()
        {
            if (!TryGetToken(out _))
                return RedirectToLogin();
            return View(ViewUserCreate, new RicRollOutCreateViewModel());
        }

        [HttpPost("RicRollOut/User/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RicRollOutCreateViewModel model, string action)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();
            if (!ModelState.IsValid)
                return View(ViewUserCreate, model);

            try
            {
                var compareUrls = await SaveFilesAsync(model.CompareWithAsIsHoldingProcessFiles);
                var stkUrls = await SaveFilesAsync(model.StkAsIsToBeFiles);

                var dto = new CreateRicRollOutRequest
                {
                    Entitas = model.Entitas ?? "",
                    JudulAplikasi = model.JudulAplikasi ?? "",
                    Hashtag = (model.Hashtags ?? new List<string>()).Select(NormalizeTag).ToList(),

                    CompareWithAsIsHoldingProcessFiles = compareUrls,
                    StkAsIsToBeFiles = stkUrls,

                    IsJoinedDomainAdPertamina = model.IsJoinedDomainAdPertamina,
                    IsUsingErpPertamina = model.IsUsingErpPertamina,
                    IsImplementedRequiredActivation = model.IsImplementedRequiredActivation,
                    HasDataCenterConnection = model.HasDataCenterConnection,
                    HasRequiredResource = model.HasRequiredResource,

                    Action = action,
                };

                await RollOutService.CreateAsync(dto, token);

                TempData["SuccessMessage"] = "RIC RollOut berhasil dibuat!";
                return RedirectToAction(nameof(UserIndex));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating RollOut");
                ModelState.AddModelError("", "Terjadi kesalahan saat membuat RollOut.");
                return View(ViewUserCreate, model);
            }
        }

        [HttpGet("RicRollOut/User/Edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var detail = await RollOutService.GetDetailByIdAsync(id, token);
            if (detail == null)
                return NotFound();

            //  cek Draft (ini enum-nya ada)
            if (detail.Status != StatusRicRollOut.Draft)
                return RejectByStatus(
                    "RollOut hanya bisa diedit kalau status masih Draft.",
                    nameof(UserIndex)
                );

            var vm = new RicRollOutCreateViewModel
            {
                Id = detail.Id,
                Entitas = detail.Entitas,
                JudulAplikasi = detail.JudulAplikasi,
                Hashtags = detail.Hashtag ?? new List<string>(),

                ExistingCompareWithAsIsHoldingProcessFileUrls =
                    detail.CompareWithAsIsHoldingProcessFiles ?? new List<string>(),
                ExistingStkAsIsToBeFileUrls = detail.StkAsIsToBeFiles ?? new List<string>(),

                IsJoinedDomainAdPertamina = detail.IsJoinedDomainAdPertamina,
                IsUsingErpPertamina = detail.IsUsingErpPertamina,
                IsImplementedRequiredActivation = detail.IsImplementedRequiredActivation,
                HasDataCenterConnection = detail.HasDataCenterConnection,
                HasRequiredResource = detail.HasRequiredResource,
            };

            ModelState.Clear();
            return View(ViewUserEdit, vm);
        }

        [HttpPost("RicRollOut/User/Edit/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            RicRollOutCreateViewModel model,
            string action
        )
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();
            if (!ModelState.IsValid)
                return View(ViewUserEdit, model);

            var existing = await RollOutService.GetDetailByIdAsync(id, token);
            if (existing == null)
                return NotFound();

            if (existing.Status != StatusRicRollOut.Draft)
                return RejectByStatus(
                    "RollOut hanya bisa diedit kalau status masih Draft.",
                    nameof(UserIndex)
                );

            try
            {
                var compareUrls = await ResolveFileUrlsAsync(
                    model.CompareWithAsIsHoldingProcessFiles,
                    model.ExistingCompareWithAsIsHoldingProcessFileUrls
                );

                var stkUrls = await ResolveFileUrlsAsync(
                    model.StkAsIsToBeFiles,
                    model.ExistingStkAsIsToBeFileUrls
                );

                var dto = new UpdateRicRollOutRequest
                {
                    Entitas = model.Entitas ?? "",
                    JudulAplikasi = model.JudulAplikasi ?? "",
                    Hashtag = (model.Hashtags ?? new List<string>()).Select(NormalizeTag).ToList(),

                    CompareWithAsIsHoldingProcessFiles = compareUrls,
                    StkAsIsToBeFiles = stkUrls,

                    IsJoinedDomainAdPertamina = model.IsJoinedDomainAdPertamina,
                    IsUsingErpPertamina = model.IsUsingErpPertamina,
                    IsImplementedRequiredActivation = model.IsImplementedRequiredActivation,
                    HasDataCenterConnection = model.HasDataCenterConnection,
                    HasRequiredResource = model.HasRequiredResource,

                    Action = action,
                };

                await RollOutService.UpdateAsync(id, dto, token);

                TempData["SuccessMessage"] = "RollOut berhasil diperbarui!";
                return RedirectToAction(nameof(UserIndex));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating RollOut {Id}", id);
                ModelState.AddModelError("", "Terjadi kesalahan saat update RollOut.");
                return View(ViewUserEdit, model);
            }
        }

        [HttpGet("RicRollOut/User/Update/{id:guid}")]
        public async Task<IActionResult> Update(Guid id)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var detail = await RollOutService.GetDetailByIdAsync(id, token);
            if (detail == null)
                return NotFound();

            //  enum lu gak punya Return_BR_To_User  pake string compare biar compile
            if (
                !string.Equals(detail.Status.ToString(), "Rejected_By_BR", StringComparison.Ordinal)
            )
                return RejectByStatus(
                    "RollOut hanya bisa diupdate kalau status Rejected_By_BR.",
                    nameof(UserIndex)
                );

            // var vm = new RicRollOutCreateViewModel
            // {
            //     Id = detail.Id,
            //     Entitas = detail.Entitas,
            //     JudulAplikasi = detail.JudulAplikasi,
            //     Hashtags = detail.Hashtag ?? new List<string>(),

            //     ExistingCompareWithAsIsHoldingProcessFileUrls =
            //         detail.CompareWithAsIsHoldingProcessFiles ?? new List<string>(),
            //     ExistingStkAsIsToBeFileUrls =
            //         detail.StkAsIsToBeFiles ?? new List<string>(),

            //     IsJoinedDomainAdPertamina = detail.IsJoinedDomainAdPertamina,
            //     IsUsingErpPertamina = detail.IsUsingErpPertamina,
            //     IsImplementedRequiredActivation = detail.IsImplementedRequiredActivation,
            //     HasDataCenterConnection = detail.HasDataCenterConnection,
            //     HasRequiredResource = detail.HasRequiredResource,
            // };

            var vm = new RicRollOutDetailViewModel
            {
                Id = detail.Id,

                // read-only header/info
                IdUser = detail.IdUser,
                UserName = detail.UserName,
                IdGroupUser = detail.IdGroupUser,
                GroupName = detail.GroupName,
                Status = detail.Status,
                CreatedAt = detail.CreatedAt,
                UpdatedAt = detail.UpdatedAt,

                // editable form
                Entitas = detail.Entitas,
                JudulAplikasi = detail.JudulAplikasi,
                Hashtags = detail.Hashtag ?? new List<string>(),

                ExistingCompareWithAsIsHoldingProcessFileUrls =
                    detail.CompareWithAsIsHoldingProcessFiles ?? new List<string>(),
                ExistingStkAsIsToBeFileUrls = detail.StkAsIsToBeFiles ?? new List<string>(),

                IsJoinedDomainAdPertamina = detail.IsJoinedDomainAdPertamina,
                IsUsingErpPertamina = detail.IsUsingErpPertamina,
                IsImplementedRequiredActivation = detail.IsImplementedRequiredActivation,
                HasDataCenterConnection = detail.HasDataCenterConnection,
                HasRequiredResource = detail.HasRequiredResource,

                // extra display
                // Reviews = reviews,
                // Histories = histories,
            };

            ModelState.Clear();
            return View(ViewUserUpdate, vm);
        }


        [HttpPost("RicRollOut/User/Update/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            Guid id,
            RicRollOutDetailViewModel model,
            string action
        )
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            //  ini penting biar view gak error walau invalid
            if (!ModelState.IsValid)
                return View(ViewUserUpdate, model);

            var existing = await RollOutService.GetDetailByIdAsync(id, token);
            if (existing == null)
                return NotFound();

            if (
                !string.Equals(
                    existing.Status.ToString(),
                    "Rejected_By_BR",
                    StringComparison.Ordinal
                )
            )
                return RejectByStatus(
                    "RollOut hanya bisa diupdate kalau status Rejected_By_BR.",
                    nameof(UserIndex)
                );

            try
            {
                var compareUrls = await ResolveFileUrlsAsync(
                    model.CompareWithAsIsHoldingProcessFiles,
                    model.ExistingCompareWithAsIsHoldingProcessFileUrls
                );

                var stkUrls = await ResolveFileUrlsAsync(
                    model.StkAsIsToBeFiles,
                    model.ExistingStkAsIsToBeFileUrls
                );

                var dto = new UpdateRicRollOutRequest
                {
                    Entitas = model.Entitas ?? "",
                    JudulAplikasi = model.JudulAplikasi ?? "",
                    Hashtag = (model.Hashtags ?? new List<string>()).Select(NormalizeTag).ToList(),

                    CompareWithAsIsHoldingProcessFiles = compareUrls,
                    StkAsIsToBeFiles = stkUrls,

                    IsJoinedDomainAdPertamina = model.IsJoinedDomainAdPertamina,
                    IsUsingErpPertamina = model.IsUsingErpPertamina,
                    IsImplementedRequiredActivation = model.IsImplementedRequiredActivation,
                    HasDataCenterConnection = model.HasDataCenterConnection,
                    HasRequiredResource = model.HasRequiredResource,

                    // Action = "updated", //  supaya API tau "save" / "submit"
                };

                //  resubmit bisa lu map ke UpdateAsync dengan Action="submit"
                // karena API lu di Update() sudah handle action save/submit
                await RollOutService.ResubmitAsync(id, dto, token);

                TempData["SuccessMessage"] = "RollOut berhasil di-resubmit!";
                return RedirectToAction(nameof(UserIndex));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error resubmitting RollOut {Id}", id);
                ModelState.AddModelError("", "Terjadi kesalahan saat resubmit RollOut.");

                //  balikin view dengan tipe model yang BENAR
                return View(ViewUserUpdate, model);
            }
        }

        [HttpPost("RicRollOut/User/Update/{id:guid}/resubmit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resubmit(Guid id, RicRollOutDetailViewModel model)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            // buang field read-only biar ModelState gak invalid
            ModelState.Remove(nameof(RicRollOutDetailViewModel.IdUser));
            ModelState.Remove(nameof(RicRollOutDetailViewModel.UserName));
            ModelState.Remove(nameof(RicRollOutDetailViewModel.IdGroupUser));
            ModelState.Remove(nameof(RicRollOutDetailViewModel.GroupName));
            ModelState.Remove(nameof(RicRollOutDetailViewModel.Status));
            ModelState.Remove(nameof(RicRollOutDetailViewModel.CreatedAt));
            ModelState.Remove(nameof(RicRollOutDetailViewModel.UpdatedAt));
            ModelState.Remove(nameof(RicRollOutDetailViewModel.Reviews));

            if (!ModelState.IsValid)
                return View(ViewUserUpdate, model);

            var existing = await RollOutService.GetDetailByIdAsync(id, token);
            if (existing == null)
                return NotFound();

            if (existing.Status != StatusRicRollOut.Rejected_By_BR)
                return RejectByStatus(
                    "RollOut hanya bisa di-resubmit kalau status Rejected_By_BR.",
                    nameof(UserIndex)
                );

            var compareUrls = await ResolveFileUrlsAsync(
                model.CompareWithAsIsHoldingProcessFiles,
                model.ExistingCompareWithAsIsHoldingProcessFileUrls
            );

            var stkUrls = await ResolveFileUrlsAsync(
                model.StkAsIsToBeFiles,
                model.ExistingStkAsIsToBeFileUrls
            );

            var dto = new UpdateRicRollOutRequest
            {
                Entitas = model.Entitas ?? "",
                JudulAplikasi = model.JudulAplikasi ?? "",
                Hashtag = (model.Hashtags ?? new List<string>()).Select(NormalizeTag).ToList(),
                CompareWithAsIsHoldingProcessFiles = compareUrls,
                StkAsIsToBeFiles = stkUrls,
                IsJoinedDomainAdPertamina = model.IsJoinedDomainAdPertamina,
                IsUsingErpPertamina = model.IsUsingErpPertamina,
                IsImplementedRequiredActivation = model.IsImplementedRequiredActivation,
                HasDataCenterConnection = model.HasDataCenterConnection,
                HasRequiredResource = model.HasRequiredResource,
            };

            await RollOutService.ResubmitAsync(id, dto, token);

            TempData["SuccessMessage"] = "RollOut berhasil di-resubmit!";
            return RedirectToAction(nameof(UserIndex));
        }

        // ========= APPROVAL =========

        [RoleRequired(Role.User_Manager, Role.User_VP)]
        [HttpGet("RicRollOut/User/Approval")]
        public async Task<IActionResult> ApprovalIndex()
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var items = await RollOutService.GetMyRollOutsAsync(token);

            //  enum lu gak punya Approval_Manager_User dll  list item status itu string, jadi aman
            var approvalItems = items
                .Where(x =>
                    x.Status == "Approval_Manager_User"
                    || x.Status == "Approval_VP_User"
                    || x.Status == "Approval_Manager_BR"
                )
                .ToList();

            return View(ViewApprovalIndex, approvalItems);
        }

        [RoleRequired(Role.User_Manager, Role.User_VP)]
        [HttpGet("RicRollOut/User/Approval/{id:guid}")]
        public async Task<IActionResult> Approval(Guid id)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var detail = await RollOutService.GetDetailByIdAsync(id, token);
            if (detail == null)
                return NotFound();

            var vm = new RicRollOutCreateViewModel
            {
                Id = detail.Id,
                Entitas = detail.Entitas,
                JudulAplikasi = detail.JudulAplikasi,
                Hashtags = detail.Hashtag ?? new List<string>(),
            };

            return View(ViewApprovalDetail, vm);
        }

        [RoleRequired(Role.User_Manager, Role.User_VP)]
        [HttpPost("RicRollOut/User/Approval/{id:guid}/approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAction(Guid id)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var ok = await RollOutService.ApproveAsync(id, token);
            if (!ok)
            {
                TempData["ErrorMessage"] =
                    "Gagal approve RollOut. Cek status/role atau pending approval belum ada.";
                return RedirectToAction(nameof(Approval), new { id });
            }

            TempData["SuccessMessage"] = "RollOut berhasil di-approve.";
            return RedirectToAction(nameof(Approval), new { id });
        }
    }
}
