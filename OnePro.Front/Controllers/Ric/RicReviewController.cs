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
        private const string ViewReviewDetail = "~/Views/Ric/Detail.cshtml";
        private const string ViewHistoryCompare = "~/Views/Ric/HistoryCompare.cshtml";

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

        [RoleRequired(Role.BR_Pic, Role.SARM_Pic, Role.ECS_Pic)]
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
            var status = (StatusRic)ric.Status;
            ViewBag.Status = status.ToString();

            var roleStr = HttpContext.Session.GetString("UserRole") ?? "";
            var canEdit = false;
            if (Enum.TryParse<Role>(roleStr, ignoreCase: true, out var role))
            {
                canEdit =
                    (role == Role.BR_Pic && (status == StatusRic.Submitted_To_BR || status == StatusRic.Review_BR || status == StatusRic.Return_ECS_To_BR || status == StatusRic.Return_SARM_To_BR)) ||
                    (role == Role.SARM_Pic && status == StatusRic.Review_SARM) ||
                    (role == Role.ECS_Pic && status == StatusRic.Review_ECS);
            }
            ViewBag.CanEdit = canEdit;
            ViewBag.UserRole = roleStr;

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
        [HttpGet("Ric/Review/Details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var ric = await RicService.GetRicByIdAsync(id, token);
            if (ric == null)
                return NotFound();

            var vm = RicMapper.MapToEditViewModel(ric);
            ViewBag.BackUrl = Url.Action(nameof(ReviewIndex)) ?? "/Ric/Review";
            ViewBag.BackLabel = "Back to Review List";
            ViewBag.Status = ((StatusRic)ric.Status).ToString();
            ViewBag.FileBase = "/Ric/Review/Files";
            ViewBag.HistoryBase = "/Ric/Review/History";

            return View(ViewReviewDetail, vm);
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
        [HttpGet("Ric/Review/History/{id:guid}/{historyId:guid}")]
        public async Task<IActionResult> HistoryCompare(Guid id, Guid historyId)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var ric = await RicService.GetRicByIdAsync(id, token);
            if (ric == null)
                return NotFound();

            var histories = ric.Histories ?? new List<RicHistoryResponse>();
            var current = histories.FirstOrDefault(h => h.Id == historyId);
            if (current == null)
                return NotFound();

            var ordered = histories.OrderBy(h => h.Version).ToList();
            var idx = ordered.FindIndex(h => h.Id == historyId);
            var prev = idx > 0 ? ordered[idx - 1] : null;

            var vm = new RicHistoryCompareViewModel
            {
                RicId = ric.Id,
                Current = current,
                Previous = prev,
                Title = "RIC History Compare",
                BackUrl = Url.Action(nameof(ReviewIndex)) ?? "/Ric/Review"
            };

            ViewBag.FileBase = "/Ric/Review/Files";
            return View(ViewHistoryCompare, vm);
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
        [HttpGet("Ric/Review/Files/{id:guid}/{kind}")]
        public async Task<IActionResult> DownloadFiles(Guid id, string kind)
        {
            if (!TryGetToken(out var token))
                return RedirectToLogin();

            var ric = await RicService.GetRicByIdAsync(id, token);
            if (ric == null)
                return NotFound();

            var urls = kind?.ToLowerInvariant() switch
            {
                "asis" => ric.AsIsProcessRasciFile,
                "tobe" => ric.ToBeProcessBusinessRasciKkiFile,
                "expected" => ric.ExcpectedCompletionTargetFile,
                _ => null
            };

            if (urls == null || urls.Count == 0)
                return NotFound("No files.");

            var zipName = $"RIC_{id}_{kind}.zip";
            return DownloadZipFromUrls(urls, zipName);
        }

        [RoleRequired(Role.BR_Pic, Role.SARM_Pic, Role.ECS_Pic)]
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
