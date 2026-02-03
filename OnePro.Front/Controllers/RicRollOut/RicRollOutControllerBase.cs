using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnePro.Front.Helpers;
using OnePro.Front.Middleware;
using OnePro.Front.Services.Interfaces;

namespace OnePro.Front.Controllers.RicRollOut
{
    [AuthRequired]
    public abstract class RicRollOutControllerBase : Controller
    {
        protected readonly IRicRollOutService RollOutService;
        protected readonly ILogger Logger;
        protected readonly IWebHostEnvironment Env;

        protected RicRollOutControllerBase(
            IRicRollOutService rollOutService,
            ILogger logger,
            IWebHostEnvironment env
        )
        {
            RollOutService = rollOutService;
            Logger = logger;
            Env = env;
        }

        protected bool TryGetToken(out string token)
        {
            token = HttpContext.Session.GetString("JwtToken") ?? "";
            return !string.IsNullOrWhiteSpace(token);
        }

        protected IActionResult RedirectToLogin() => RedirectToAction("Login", "Auth");

        protected IActionResult RejectByStatus(string message, string redirectAction)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(redirectAction);
        }

        protected async Task<List<string>> SaveFilesAsync(IEnumerable<IFormFile>? files)
        {
            var uploaded = files?.Where(f => f != null && f.Length > 0).ToList();
            if (uploaded is not { Count: > 0 })
                return new List<string>();

            var coll = new FormFileCollection();
            foreach (var f in uploaded)
                coll.Add(f);

            var saved = await FileStorageHelper.SaveRicFilesAsync(coll, Env.WebRootPath, Logger);

            return saved?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();
        }

        protected async Task<List<string>> ResolveFileUrlsAsync(
            IEnumerable<IFormFile>? newFiles,
            List<string>? existingUrls
        )
        {
            var uploaded = newFiles?.Where(f => f != null && f.Length > 0).ToList();
            if (uploaded is { Count: > 0 })
            {
                var coll = new FormFileCollection();
                foreach (var f in uploaded)
                    coll.Add(f);

                var saved = await FileStorageHelper.SaveRicFilesAsync(coll, Env.WebRootPath, Logger);
                return saved?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();
            }

            return existingUrls?.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList()
                ?? new List<string>();
        }

        protected static string NormalizeTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return "";

            var t = tag.Trim();
            if (t.StartsWith("#")) t = t.Substring(1);

            t = t.Trim().ToLowerInvariant();
            t = Regex.Replace(t, @"\s+", "_");
            t = Regex.Replace(t, @"[^a-z0-9_\-]", "");

            return t;
        }

        protected IActionResult DownloadZipFromUrls(List<string>? urls, string zipFileName)
        {
            var list = (urls ?? new List<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (list.Count == 0)
                return NotFound("No files.");

            using var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var url in list)
                {
                    var fullPath = TryMapUrlToPath(url);
                    if (string.IsNullOrWhiteSpace(fullPath) || !System.IO.File.Exists(fullPath))
                        continue;

                    var entryName = Path.GetFileName(fullPath);
                    entryName = EnsureUniqueName(entryName, usedNames);

                    var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    using var fs = System.IO.File.OpenRead(fullPath);
                    fs.CopyTo(entryStream);
                }
            }

            ms.Position = 0;
            return File(ms.ToArray(), "application/zip", zipFileName);
        }

        private string? TryMapUrlToPath(string url)
        {
            try
            {
                var relative = (url ?? "")
                    .Trim()
                    .TrimStart('~')
                    .TrimStart('/')
                    .Replace('/', Path.DirectorySeparatorChar);

                var fullPath = Path.GetFullPath(Path.Combine(Env.WebRootPath, relative));
                var webRootFull = Path.GetFullPath(Env.WebRootPath);

                if (!fullPath.StartsWith(webRootFull, StringComparison.OrdinalIgnoreCase))
                    return null;

                return fullPath;
            }
            catch
            {
                return null;
            }
        }

        private static string EnsureUniqueName(string name, HashSet<string> used)
        {
            if (used.Add(name))
                return name;

            var baseName = Path.GetFileNameWithoutExtension(name);
            var ext = Path.GetExtension(name);
            var i = 2;
            string candidate;

            do
            {
                candidate = $"{baseName} ({i}){ext}";
                i++;
            } while (!used.Add(candidate));

            return candidate;
        }
    }
}
