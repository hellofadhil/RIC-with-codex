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
    public class RicRollOutReviewController : RicRollOutControllerBase
    {
        private const string ViewReviewIndex = "~/Views/RicRollOut/Review/Index.cshtml";
        private const string ViewReviewForm = "~/Views/RicRollOut/Review/Form.cshtml";

        public RicRollOutReviewController(
            IRicRollOutService rollOutService,
            ILogger<RicRollOutReviewController> logger,
            IWebHostEnvironment env
        )
            : base(rollOutService, logger, env) { }

        [RoleRequired(
            Role.BR_Pic,
            Role.BR_Member,
            Role.BR_Manager,
            Role.BR_VP,
            Role.SARM_Pic,
            Role.SARM_Member,
            Role.SARM_Manager,
            Role.SARM_VP,
            Role.ECS_Pic,
            Role.ECS_Member,
            Role.ECS_Manager,
            Role.ECS_VP
        )]
        [HttpGet("RicRollOut/Review")]
        public async Task<IActionResult> ReviewIndex()
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");

            var items = await RollOutService.GetMyRollOutsAsync(token);
            return View(ViewReviewIndex, items);
        }

        [RoleRequired(
            Role.BR_Pic,
            Role.BR_Member,
            Role.BR_Manager,
            Role.BR_VP,
            Role.SARM_Pic,
            Role.SARM_Member,
            Role.SARM_Manager,
            Role.SARM_VP,
            Role.ECS_Pic,
            Role.ECS_Member,
            Role.ECS_Manager,
            Role.ECS_VP
        )]
        [HttpGet("RicRollOut/Review/Files/{id:guid}/{kind}")]
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

        [RoleRequired(
            Role.BR_Pic,
            Role.BR_Member,
            Role.BR_Manager,
            Role.BR_VP,
            Role.SARM_Pic,
            Role.SARM_Member,
            Role.SARM_Manager,
            Role.SARM_VP,
            Role.ECS_Pic,
            Role.ECS_Member,
            Role.ECS_Manager,
            Role.ECS_VP
        )]
        [HttpGet("RicRollOut/Review/{id:guid}")]
        public async Task<IActionResult> ReviewEdit(Guid id)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var detail = await RollOutService.GetDetailByIdAsync(id, token);
            if (detail == null)
                return NotFound();

            // kalau lu punya mapper RollOut, pakai itu.
            // sementara: langsung lempar viewmodel kosong yang udah diisi manual.
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

            return View(ViewReviewForm, vm);
        }

        [RoleRequired(
            Role.BR_Pic,
            Role.BR_Member,
            Role.BR_Manager,
            Role.BR_VP,
            Role.SARM_Pic,
            Role.SARM_Member,
            Role.SARM_Manager,
            Role.SARM_VP,
            Role.ECS_Pic,
            Role.ECS_Member,
            Role.ECS_Manager,
            Role.ECS_VP
        )]
        [HttpPost("RicRollOut/Review")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewForm(
            RicRollOutCreateViewModel model,
            string action,
            string? note
        )
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            action = (action ?? "").Trim().ToLowerInvariant();

            if (!ModelState.IsValid)
                return View(ViewReviewForm, model);

            if (action == "reject")
            {
                if (string.IsNullOrWhiteSpace(note))
                {
                    ModelState.AddModelError("", "Catatan penolakan wajib diisi.");
                    return View(ViewReviewForm, model);
                }

                await RollOutService.RejectAsync(model.Id, note, token);
                return RedirectToAction(nameof(ReviewIndex));
            }

            if (action == "approve")
            {
                var compareUrls = await ResolveFileUrlsAsync(
                    model.CompareWithAsIsHoldingProcessFiles,
                    model.ExistingCompareWithAsIsHoldingProcessFileUrls
                );

                var stkUrls = await ResolveFileUrlsAsync(
                    model.StkAsIsToBeFiles,
                    model.ExistingStkAsIsToBeFileUrls
                );

                var req = new UpdateRicRollOutRequest
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

                var ok = await RollOutService.ForwardAsync(model.Id, req, token);
                if (!ok)
                {
                    ModelState.AddModelError("", "Gagal forward RollOut ke API.");
                    return View(ViewReviewForm, model);
                }

                return RedirectToAction(nameof(ReviewIndex));
            }

            ModelState.AddModelError("", "Action tidak valid.");
            return View(ViewReviewForm, model);
        }
    }
}
