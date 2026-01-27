// using System.Text.RegularExpressions;
// using Core.Models.Enums;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using OnePro.Front.Helpers;
// using OnePro.Front.Mappers;
// using OnePro.Front.Middleware;
// using OnePro.Front.Models;
// using OnePro.Front.Services.Interfaces;

// namespace OnePro.Front.Controllers
// {
//     [AuthRequired]
//     public class RicController : Controller
//     {
//         private readonly IRicService _ricService;
//         private readonly ILogger<RicController> _logger;
//         private readonly IWebHostEnvironment _env;

//         // View paths biar gak typo / duplikat string
//         private const string ViewUserIndex = "~/Views/Ric/User/Index.cshtml";
//         private const string ViewUserCreate = "~/Views/Ric/User/Create.cshtml";
//         private const string ViewUserEdit = "~/Views/Ric/User/Edit.cshtml";
//         private const string ViewUserUpdate = "~/Views/Ric/User/Update.cshtml";
//         private const string ViewReviewIndex = "~/Views/Ric/Review/Index.cshtml";
//         private const string ViewReviewForm = "~/Views/Ric/Review/Form.cshtml";

//         public RicController(
//             IRicService ricService,
//             ILogger<RicController> logger,
//             IWebHostEnvironment env
//         )
//         {
//             _ricService = ricService;
//             _logger = logger;
//             _env = env;
//         }

//         #region User Views

//         // [RoleRequired(0, 1, 2, 3)]
//         [RoleRequired(Role.User_Member, Role.User_Pic, Role.User_Manager, Role.User_VP)]
//         [HttpGet("ric/user")]
//         public async Task<IActionResult> UserIndex()
//         {
//             if (!TryGetToken(out var token))
//                 return RedirectToLogin();

//             var rics = await _ricService.GetMyRicsAsync(token);
//             return View(ViewUserIndex, rics);
//         }

//         [HttpGet("ric/br/review")]
//         public async Task<IActionResult> ReviewIndex()
//         {
//             if (!TryGetToken(out var token))
//                 return RedirectToLogin();

//             var userRole = HttpContext.Session.GetString("UserRole");
//             _logger.LogInformation(
//                 "ReviewIndex accessed. UserRole from session: {UserRole}",
//                 userRole
//             );

//             ViewBag.UserRole = userRole;

//             var rics = await _ricService.GetMyRicsAsync(token);
//             return View(ViewReviewIndex, rics);
//         }

//         // [RoleRequired(1, 4)]
//         [HttpGet("ric/user/create")]
//         public IActionResult Create()
//         {
//             if (!TryGetToken(out _))
//                 return RedirectToLogin();

//             return View(ViewUserCreate, new RicCreateViewModel());
//         }

//         [HttpPost("ric/user/create")]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Create(RicCreateViewModel model, string action)
//         {
//             if (!TryGetToken(out var token))
//                 return RedirectToLogin();

//             if (!ModelState.IsValid)
//                 return View(ViewUserCreate, model);

//             try
//             {
//                 var dto = await RicMapper.MapToCreateRequestAsync(model, action, SaveFilesAsync);

//                 await _ricService.CreateRicAsync(dto, token);

//                 TempData["SuccessMessage"] = "RIC berhasil dibuat!";
//                 return RedirectToAction(nameof(UserIndex));
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error creating RIC");
//                 ModelState.AddModelError(string.Empty, "Terjadi kesalahan saat membuat RIC.");
//                 return View(ViewUserCreate, model);
//             }
//         }

//         [HttpGet("ric/user/edit/{id:guid}")]
//         public async Task<IActionResult> Edit(Guid id)
//         {
//             if (!TryGetToken(out var token))
//                 return RedirectToLogin();

//             var ric = await _ricService.GetRicByIdAsync(id, token);
//             if (ric == null)
//                 return NotFound();

//             if ((StatusRic)ric.Status != StatusRic.Draft)
//                 return RejectByStatus(
//                     "RIC hanya bisa diedit kalau status masih Draft.",
//                     nameof(UserIndex)
//                 );

//             var vm = RicMapper.MapToEditViewModel(ric);
//             ModelState.Clear();
//             return View(ViewUserEdit, vm);
//         }

//         [HttpPost("ric/user/edit/{id:guid}")]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Edit(Guid id, RicCreateViewModel model, string action)
//         {
//             if (!TryGetToken(out var token))
//                 return RedirectToLogin();

//             if (!ModelState.IsValid)
//                 return View(ViewUserEdit, model);

//             var existing = await _ricService.GetRicByIdAsync(id, token);
//             if (existing == null)
//                 return NotFound();

//             if ((StatusRic)existing.Status != StatusRic.Draft)
//                 return RejectByStatus(
//                     "RIC hanya bisa diedit kalau status masih Draft.",
//                     nameof(UserIndex)
//                 );

//             try
//             {
//                 var dto = await RicMapper.MapToUpdateRequestAsync(
//                     id,
//                     model,
//                     action,
//                     existing,
//                     SaveFilesAsync
//                 );

//                 await _ricService.UpdateRicAsync(id, dto, token);

//                 TempData["SuccessMessage"] = "RIC berhasil diperbarui!";
//                 return RedirectToAction(nameof(UserIndex));
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error updating RIC {Id}", id);
//                 ModelState.AddModelError(string.Empty, "Terjadi kesalahan saat update RIC.");
//                 return View(ViewUserEdit, model);
//             }
//         }

//         [HttpGet("ric/user/update/{id:guid}")]
//         public async Task<IActionResult> Update(Guid id)
//         {
//             if (!TryGetToken(out var token))
//                 return RedirectToLogin();

//             var ric = await _ricService.GetRicByIdAsync(id, token);
//             if (ric == null)
//                 return NotFound();

//             if ((StatusRic)ric.Status != StatusRic.Return_BR_To_User)
//                 return RejectByStatus(
//                     "RIC hanya bisa diupdate kalau status masih Return_BR_To_User.",
//                     nameof(UserIndex)
//                 );

//             var vm = RicMapper.MapToEditViewModel(ric);
//             ModelState.Clear();
//             return View(ViewUserUpdate, vm);
//         }

//         [HttpPost("ric/user/update/{id:guid}")]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Update(Guid id, RicCreateViewModel model, string action)
//         {
//             if (!TryGetToken(out var token))
//                 return RedirectToLogin();

//             if (!ModelState.IsValid)
//                 return View(ViewUserUpdate, model);

//             var existing = await _ricService.GetRicByIdAsync(id, token);
//             if (existing == null)
//                 return NotFound();

//             if ((StatusRic)existing.Status != StatusRic.Return_BR_To_User)
//                 return RejectByStatus(
//                     "RIC hanya bisa diupdate kalau status masih Return_BR_To_User.",
//                     nameof(UserIndex)
//                 );

//             try
//             {
//                 // action di UI sekarang cuma "submit"
//                 var dto = await RicMapper.MapToResubmitRequestAsync(
//                     model,
//                     action,
//                     existing,
//                     SaveFilesAsync
//                 );

//                 await _ricService.ResubmitRicAsync(id, dto, token);

//                 TempData["SuccessMessage"] = "RIC berhasil di-resubmit!";
//                 return RedirectToAction(nameof(UserIndex));
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error resubmitting RIC {Id}", id);
//                 ModelState.AddModelError(string.Empty, "Terjadi kesalahan saat resubmit RIC.");
//                 return View(ViewUserUpdate, model);
//             }
//         }

//         #endregion

//         #region Review Views (BR/SARM/ECS)

//         // [RoleRequired(4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15)]
// //         [RoleRequired(
// //             Role.BR_Pic, Role.BR_Member, Role.BR_Manager, Role.BR_VP,
// //             Role.SARM_Pic, Role.SARM_Member, Role.SARM_Manager, Role.SARM_VP,
// //             Role.ECS_Pic, Role.ECS_Member, Role.ECS_Manager, Role.ECS_VP
// //         )]

// //         [HttpGet("Ric/Review")]
// //         public async Task<IActionResult> ReviewEdit(Guid id)
// //         {
// //             if (!TryGetToken(out var token))
// //                 return RedirectToLogin();

// //             var ric = await _ricService.GetRicByIdAsync(id, token);
// //             if (ric == null)
// //                 return NotFound();

// //             var vm = RicMapper.MapToEditViewModel(ric);
// //             vm.Id = ric.Id;

// //             return View(ViewReviewForm, vm);
// //         }

// //         // [RoleRequired(4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15)]
// //         [RoleRequired(
// //     Role.BR_Pic, Role.BR_Member, Role.BR_Manager, Role.BR_VP,
// //     Role.SARM_Pic, Role.SARM_Member, Role.SARM_Manager, Role.SARM_VP,
// //     Role.ECS_Pic, Role.ECS_Member, Role.ECS_Manager, Role.ECS_VP
// // )]

// //         [HttpPost("ric/review")]
// //         [ValidateAntiForgeryToken]
// //         public async Task<IActionResult> ReviewForm(
// //             RicCreateViewModel model,
// //             string action,
// //             string? note
// //         )
// //         {
// //             if (!TryGetToken(out var token))
// //                 return RedirectToLogin();

// //             action = action.Trim().ToLowerInvariant();

// //             if (!ModelState.IsValid)
// //             {
// //                 return View(ViewReviewForm, model);
// //             }

// //             if (action == "reject")
// //             {
// //                 if (string.IsNullOrWhiteSpace(note))
// //                 {
// //                     ModelState.AddModelError("", "Catatan penolakan wajib diisi.");
// //                     return View(ViewReviewForm, model);
// //                 }

// //                 await _ricService.RejectAsync(model.Id, note, token);
// //                 return RedirectToAction(nameof(ReviewIndex));
// //             }

// //             if (action == "approve")
// //             {
// //                 var asIsUrls = await ResolveFileUrlsAsync(
// //                     model.AsIsRasciFiles,
// //                     model.ExistingAsIsFileUrls
// //                 );
// //                 var toBeUrls = await ResolveFileUrlsAsync(
// //                     model.ToBeProcessFiles,
// //                     model.ExistingToBeFileUrls
// //                 );
// //                 var expectedUrls = await ResolveFileUrlsAsync(
// //                     model.ExpectedCompletionFiles,
// //                     model.ExistingExpectedCompletionFileUrls
// //                 );

// //                 var req = new FormRicResubmitRequest
// //                 {
// //                     Judul = model.JudulPermintaan ?? "",
// //                     Hastag = (model.Hashtags ?? new List<string>()).Select(NormalizeTag).ToList(),

// //                     AsIsProcessRasciFile = asIsUrls,

// //                     Permasalahan = model.Permasalahan ?? "",
// //                     DampakMasalah = model.DampakMasalah ?? "",
// //                     FaktorPenyebabMasalah = model.FaktorPenyebab ?? "",
// //                     SolusiSaatIni = model.SolusiSaatIni ?? "",

// //                     AlternatifSolusi = model.Alternatifs ?? new List<string>(),

// //                     ToBeProcessBusinessRasciKkiFile = toBeUrls,
// //                     PotensiValueCreation = model.PotentialValue ?? "",
// //                     ExcpectedCompletionTargetFile = expectedUrls,
// //                     HasilSetelahPerbaikan = model.HasilSetelahPerbaikan ?? "",

// //                     Status = 0, // TODO: set sesuai status Forward di backend lo
// //                 };

// //                 var ok = await _ricService.ForwardAsync(model.Id, req, token);
// //                 if (!ok)
// //                 {
// //                     ModelState.AddModelError("", "Gagal forward RIC ke API.");
// //                     return View(ViewReviewForm, model);
// //                 }

// //                 return RedirectToAction(nameof(ReviewIndex));
// //             }

// //             ModelState.AddModelError("", "Action tidak valid.");
// //             return View(ViewReviewForm, model);
// //         }

//         #endregion

//         #region Private Helpers

//         // private bool TryGetToken(out string token)
//         // {
//         //     token = HttpContext.Session.GetString("JwtToken") ?? "";
//         //     return !string.IsNullOrWhiteSpace(token);
//         // }

//         private IActionResult RedirectToLogin() => RedirectToAction("Login", "Auth");

//         private IActionResult RejectByStatus(string message, string redirectAction)
//         {
//             TempData["ErrorMessage"] = message;
//             return RedirectToAction(redirectAction);
//         }

//         // private async Task<List<string>> SaveFilesAsync(IEnumerable<IFormFile>? files)
//         // {
//         //     var uploaded = files?.Where(f => f != null && f.Length > 0).ToList();
//         //     if (uploaded is not { Count: > 0 })
//         //         return new List<string>();

//         //     var coll = new FormFileCollection();
//         //     foreach (var f in uploaded)
//         //         coll.Add(f);

//         //     var saved = await FileStorageHelper.SaveRicFilesAsync(coll, _env.WebRootPath, _logger);

//         //     return saved?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();
//         // }

//         // private async Task<List<string>> ResolveFileUrlsAsync(
//         //     IEnumerable<IFormFile>? newFiles,
//         //     List<string>? existingUrls
//         // )
//         // {
//         //     // kalau ada file baru -> replace
//         //     var uploaded = newFiles?.Where(f => f != null && f.Length > 0).ToList();
//         //     if (uploaded is { Count: > 0 })
//         //     {
//         //         var coll = new FormFileCollection();
//         //         foreach (var f in uploaded)
//         //             coll.Add(f);

//         //         var saved = await FileStorageHelper.SaveRicFilesAsync(
//         //             coll,
//         //             _env.WebRootPath,
//         //             _logger
//         //         );

//         //         return saved?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()
//         //             ?? new List<string>();
//         //     }

//         //     // kalau ga upload baru -> keep existing
//         //     return existingUrls?.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList()
//         //         ?? new List<string>();
//         // }

//         // private static string NormalizeTag(string tag)
//         // {
//         //     if (string.IsNullOrWhiteSpace(tag))
//         //         return "";

//         //     var t = tag.Trim();

//         //     // buang prefix '#'
//         //     if (t.StartsWith("#"))
//         //         t = t.Substring(1);

//         //     t = t.Trim().ToLowerInvariant();

//         //     // ganti spasi jadi underscore
//         //     t = Regex.Replace(t, @"\s+", "_");

//         //     // whitelist: huruf/angka/_/-
//         //     t = Regex.Replace(t, @"[^a-z0-9_\-]", "");

//         //     return t;
//         // }

//         #endregion
//     }
// }
