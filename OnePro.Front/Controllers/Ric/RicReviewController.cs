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
    public class RicReviewController : RicControllerBase
    {
        private const string ViewReviewIndex = "~/Views/Ric/Review/Index.cshtml";
        private const string ViewReviewForm = "~/Views/Ric/Review/Form.cshtml";

        public RicReviewController(
            IRicService ricService,
            ILogger<RicReviewController> logger,
            IWebHostEnvironment env
        )
            : base(ricService, logger, env) { }

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
        [HttpGet("Ric/Review")]
        public async Task<IActionResult> ReviewIndex(string? q, string? status)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var userRole = HttpContext.Session.GetString("UserRole");
            // Logger.LogInformation("ReviewIndex accessed. UserRole from session: {UserRole}", userRole);

            ViewBag.UserRole = userRole;

            var rics = await RicService.GetMyRicsAsync(token, q, 50);
            if (!string.IsNullOrWhiteSpace(status))
            {
                rics = rics.Where(x =>
                        string.Equals(x.Status, status, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();
            }

            rics = rics.Take(10).ToList();
            return View(ViewReviewIndex, rics);
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
        [HttpGet("Ric/Review/{id:guid}")]
        public async Task<IActionResult> ReviewEdit(Guid id)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var ric = await RicService.GetRicByIdAsync(id, token);
            if (ric == null)
                return NotFound();

            var vm = RicMapper.MapToEditViewModel(ric);
            vm.Id = ric.Id;

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
        [HttpPost("Ric/Review")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewForm(
            RicCreateViewModel model,
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

                await RicService.RejectAsync(model.Id, note, token);
                return RedirectToAction(nameof(ReviewIndex));
            }

            if (action == "approve")
            {
                var asIsUrls = await ResolveFileUrlsAsync(
                    model.AsIsRasciFiles,
                    model.ExistingAsIsFileUrls
                );
                var toBeUrls = await ResolveFileUrlsAsync(
                    model.ToBeProcessFiles,
                    model.ExistingToBeFileUrls
                );
                var expectedUrls = await ResolveFileUrlsAsync(
                    model.ExpectedCompletionFiles,
                    model.ExistingExpectedCompletionFileUrls
                );

                var req = new FormRicResubmitRequest
                {
                    Judul = model.JudulPermintaan ?? "",
                    Hastag = (model.Hashtags ?? new List<string>()).Select(NormalizeTag).ToList(),

                    AsIsProcessRasciFile = asIsUrls,
                    Permasalahan = model.Permasalahan ?? "",
                    DampakMasalah = model.DampakMasalah ?? "",
                    FaktorPenyebabMasalah = model.FaktorPenyebab ?? "",
                    SolusiSaatIni = model.SolusiSaatIni ?? "",
                    AlternatifSolusi = model.Alternatifs ?? new List<string>(),

                    ToBeProcessBusinessRasciKkiFile = toBeUrls,
                    PotensiValueCreation = model.PotentialValue ?? "",
                    ExcpectedCompletionTargetFile = expectedUrls,
                    HasilSetelahPerbaikan = model.HasilSetelahPerbaikan ?? "",

                    Status = 0, // TODO: mapping status forward
                };

                var ok = await RicService.ForwardAsync(model.Id, req, token);
                if (!ok)
                {
                    ModelState.AddModelError("", "Gagal forward RIC ke API.");
                    return View(ViewReviewForm, model);
                }

                return RedirectToAction(nameof(ReviewIndex));
            }

            ModelState.AddModelError("", "Action tidak valid.");
            return View(ViewReviewForm, model);
        }
    }
}
