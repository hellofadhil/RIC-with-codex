using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OnePro.Front.Helpers
{
    public static class FileStorageHelper
    {
        public static async Task<List<string>> SaveRicFilesAsync(
            IEnumerable<IFormFile>? files,
            string webRootPath,
            ILogger logger
        )
        {
            var result = new List<string>();

            if (files == null)
                return result;

            var uploadFolder = Path.Combine(webRootPath, "uploads", "ric");
            Directory.CreateDirectory(uploadFolder);

            foreach (var file in files.Where(f => f != null && f.Length > 0))
            {
                var fileName = GenerateUniqueFileName(file.FileName);
                var filePath = Path.Combine(uploadFolder, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                logger.LogInformation("RIC file saved: {FileName}", fileName);
                result.Add($"/uploads/ric/{fileName}");
            }

            return result;
        }

        private static string GenerateUniqueFileName(string originalFileName)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var extension = Path.GetExtension(originalFileName);
            return $"{timestamp}_{uniqueId}{extension}";
        }
    }
}
