using Core.Models.Enums;
using OnePro.Front.Models;

namespace OnePro.Front.Mappers
{
    public static class RicMapper
    {
        private const string ActionSubmit = "submit";

        public static async Task<FormRicCreateRequest> MapToCreateRequestAsync(
            RicCreateViewModel model,
            string action,
            Func<IEnumerable<IFormFile>?, Task<List<string>>> saveFilesAsync
        )
        {
            var (asIsUrls, toBeUrls, expectedUrls) = await SaveAllFilesAsync(model, saveFilesAsync);
            var status = ResolveStatus(action);

            return new FormRicCreateRequest
            {
                Judul = model.JudulPermintaan,
                Hastag = model.Hashtags,

                AsIsProcessRasciFile = ToNullableList(asIsUrls),

                Permasalahan = model.Permasalahan,
                DampakMasalah = model.DampakMasalah,
                FaktorPenyebabMasalah = model.FaktorPenyebab,
                SolusiSaatIni = model.SolusiSaatIni,

                AlternatifSolusi = model.Alternatifs,

                ToBeProcessBusinessRasciKkiFile = ToNullableList(toBeUrls),

                PotensiValueCreation = model.PotentialValue,

                ExcpectedCompletionTargetFile = ToNullableList(expectedUrls),

                HasilSetelahPerbaikan = model.HasilSetelahPerbaikan,
                Status = (int)status,
            };
        }

        public static async Task<FormRicUpdateRequest> MapToUpdateRequestAsync(
            Guid id,
            RicCreateViewModel model,
            string action,
            RicDetailResponse existing,
            Func<IEnumerable<IFormFile>?, Task<List<string>>> saveFilesAsync
        )
        {
            var (asIsUrls, toBeUrls, expectedUrls) = await SaveAllFilesAsync(model, saveFilesAsync);
            var status = ResolveStatus(action);

            return new FormRicUpdateRequest
            {
                Id = id,
                Judul = model.JudulPermintaan,
                Hastag = model.Hashtags,

                AsIsProcessRasciFile = PreferNewOrExisting(asIsUrls, existing.AsIsProcessRasciFile),

                Permasalahan = model.Permasalahan,
                DampakMasalah = model.DampakMasalah,
                FaktorPenyebabMasalah = model.FaktorPenyebab,
                SolusiSaatIni = model.SolusiSaatIni,

                AlternatifSolusi = model.Alternatifs,

                ToBeProcessBusinessRasciKkiFile = PreferNewOrExisting(
                    toBeUrls,
                    existing.ToBeProcessBusinessRasciKkiFile
                ),

                PotensiValueCreation = model.PotentialValue,

                ExcpectedCompletionTargetFile = PreferNewOrExisting(
                    expectedUrls,
                    existing.ExcpectedCompletionTargetFile
                ),

                HasilSetelahPerbaikan = model.HasilSetelahPerbaikan,
                Status = (int)status,
            };
        }

        public static async Task<FormRicResubmitRequest> MapToResubmitRequestAsync(
            RicCreateViewModel model,
            string action,
            RicDetailResponse existing,
            Func<IEnumerable<IFormFile>?, Task<List<string>>> saveFilesAsync
        )
        {
            var (asIsUrls, toBeUrls, expectedUrls) = await SaveAllFilesAsync(model, saveFilesAsync);
            var status = ResolveStatus(action);

            return new FormRicResubmitRequest
            {
                Judul = model.JudulPermintaan,
                Hastag = model.Hashtags,

                AsIsProcessRasciFile = PreferNewOrExisting(asIsUrls, existing.AsIsProcessRasciFile),

                Permasalahan = model.Permasalahan,
                DampakMasalah = model.DampakMasalah,
                FaktorPenyebabMasalah = model.FaktorPenyebab,
                SolusiSaatIni = model.SolusiSaatIni,

                AlternatifSolusi = model.Alternatifs,

                ToBeProcessBusinessRasciKkiFile = PreferNewOrExisting(
                    toBeUrls,
                    existing.ToBeProcessBusinessRasciKkiFile
                ),

                PotensiValueCreation = model.PotentialValue,

                ExcpectedCompletionTargetFile = PreferNewOrExisting(
                    expectedUrls,
                    existing.ExcpectedCompletionTargetFile
                ),

                HasilSetelahPerbaikan = model.HasilSetelahPerbaikan,
                Status = (int)status,
            };
        }

        // public static RicCreateViewModel MapToEditViewModel(RicDetailResponse ric)
        // {
        //     return new RicCreateViewModel
        //     {
        //         Id = ric.Id,
        //         JudulPermintaan = ric.Judul,
        //         Hashtags = ric.Hastag ?? new List<string>(),

        //         Permasalahan = ric.Permasalahan ?? string.Empty,
        //         DampakMasalah = ric.DampakMasalah ?? string.Empty,
        //         FaktorPenyebab = ric.FaktorPenyebabMasalah ?? string.Empty,
        //         SolusiSaatIni = ric.SolusiSaatIni ?? string.Empty,

        //         Alternatifs = ric.AlternatifSolusi ?? new List<string>(),
        //         PotentialValue = ric.PotensiValueCreation,
        //         HasilSetelahPerbaikan = ric.HasilSetelahPerbaikan ?? string.Empty,

        //         ExistingAsIsFileUrls = ric.AsIsProcessRasciFile,
        //         ExistingToBeFileUrls = ric.ToBeProcessBusinessRasciKkiFile,
        //         ExistingExpectedCompletionFileUrls = ric.ExcpectedCompletionTargetFile,
        //     };
        // }

        public static RicCreateViewModel MapToEditViewModel(RicDetailResponse ric)
        {
            return new RicCreateViewModel
            {
                Id = ric.Id,
                JudulPermintaan = ric.Judul,
                Hashtags = ric.Hastag ?? new List<string>(),

                Permasalahan = ric.Permasalahan ?? string.Empty,
                DampakMasalah = ric.DampakMasalah ?? string.Empty,
                FaktorPenyebab = ric.FaktorPenyebabMasalah ?? string.Empty,
                SolusiSaatIni = ric.SolusiSaatIni ?? string.Empty,

                Alternatifs = ric.AlternatifSolusi ?? new List<string>(),
                PotentialValue = ric.PotensiValueCreation,
                HasilSetelahPerbaikan = ric.HasilSetelahPerbaikan ?? string.Empty,

                ExistingAsIsFileUrls = ric.AsIsProcessRasciFile,
                ExistingToBeFileUrls = ric.ToBeProcessBusinessRasciKkiFile,
                ExistingExpectedCompletionFileUrls = ric.ExcpectedCompletionTargetFile,

                // Include review + history
                Reviews = ric.Reviews ?? new List<ReviewRicResponse>(),
                Histories = ric.Histories ?? new List<RicHistoryResponse>(),
            };
        }

        #region Private Helpers

        private static StatusRic ResolveStatus(string action) =>
            IsSubmit(action) ? StatusRic.Submitted_To_BR : StatusRic.Draft;

        private static bool IsSubmit(string action) =>
            string.Equals(
                (action ?? string.Empty).Trim(),
                ActionSubmit,
                StringComparison.OrdinalIgnoreCase
            );

        private static async Task<(
            List<string> asIs,
            List<string> toBe,
            List<string> expected
        )> SaveAllFilesAsync(
            RicCreateViewModel model,
            Func<IEnumerable<IFormFile>?, Task<List<string>>> saveFilesAsync
        )
        {
            // Normalize: saveFilesAsync mungkin return null? kita jaga-jaga jadi always list
            var asIs = await SaveOrEmptyAsync(() => saveFilesAsync(model.AsIsRasciFiles));
            var toBe = await SaveOrEmptyAsync(() => saveFilesAsync(model.ToBeProcessFiles));
            var expected = await SaveOrEmptyAsync(() =>
                saveFilesAsync(model.ExpectedCompletionFiles)
            );

            return (asIs, toBe, expected);
        }

        private static async Task<List<string>> SaveOrEmptyAsync(Func<Task<List<string>>> save) =>
            (await save()) ?? new List<string>();

        private static List<string>? ToNullableList(List<string> urls) =>
            urls != null && urls.Count > 0 ? urls : null;

        private static List<string>? PreferNewOrExisting(
            List<string> newUrls,
            List<string>? existingUrls
        ) => newUrls != null && newUrls.Count > 0 ? newUrls : existingUrls;
    }

        #endregion
}
