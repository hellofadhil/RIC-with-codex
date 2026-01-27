using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Core.Models;
using Core.Models.Entities;
using Core.Models.Enums;
using Core.RequestModels.Ric;
using Microsoft.EntityFrameworkCore;
using OnePro.API.Interfaces;

namespace OnePro.API.Repositories
{
    public class RicRepository : IRicRepository
    {
        private readonly OneProDbContext _context;

        // Group special reviewers
        private static readonly Guid GroupBrId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        private static readonly Guid GroupSarmId = Guid.Parse(
            "30000000-0000-0000-0000-000000000003"
        );
        private static readonly Guid GroupEcsId = Guid.Parse(
            "40000000-0000-0000-0000-000000000004"
        );

        private static readonly JsonSerializerOptions EditedFieldsJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public RicRepository(OneProDbContext context)
        {
            _context = context;
        }

        public async Task<List<RicListItemResponse>> GetAllByGroupAsync(
            Guid groupId,
            string? q = null,
            int limit = 10
        )
        {
            var query =
                from r in _context.FormRics.AsNoTracking()
                join u in _context.Users.AsNoTracking() on r.IdUser equals u.Id into userJoined
                from user in userJoined.DefaultIfEmpty()
                select new { Ric = r, User = user };

            // CASE 1: Group spesial (BR / SARM / ECS) => mode reviewer
            if (groupId == GroupBrId)
            {
                query = query.Where(x =>
                    x.Ric.Status == StatusRic.Submitted_To_BR
                    || x.Ric.Status == StatusRic.Review_BR
                    || x.Ric.Status == StatusRic.Return_SARM_To_BR
                    || x.Ric.Status == StatusRic.Return_ECS_To_BR
                );
            }
            else if (groupId == GroupSarmId)
            {
                query = query.Where(x => x.Ric.Status == StatusRic.Review_SARM);
            }
            else if (groupId == GroupEcsId)
            {
                query = query.Where(x => x.Ric.Status == StatusRic.Review_ECS);
            }
            // CASE 2: Group biasa => mode user divisi (track semua status)
            else
            {
                query = query.Where(x => x.Ric.IdGroupUser == groupId);
            }

            var items = await query
                .OrderByDescending(x => x.Ric.UpdatedAt)
                .Select(x => new RicListItemResponse
                {
                    Id = x.Ric.Id,
                    Judul = x.Ric.Judul,
                    Permasalahan = x.Ric.Permasalahan,
                    Hastag = x.Ric.Hastag,
                    UserName = x.User != null ? x.User.Name : null,
                    Status = x.Ric.Status.ToString(),
                    UpdatedAt = x.Ric.UpdatedAt,
                })
                .ToListAsync();

            items = ApplySearch(items, q);
            return items.Take(NormalizeLimit(limit)).ToList();
        }

        public async Task<FormRic?> GetByIdAsync(Guid id)
        {
            return await _context.FormRics!.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<FormRicDetailResponse?> GetDetailByIdAsync(Guid id)
        {
            var query =
                from r in _context.FormRics.AsNoTracking()
                where r.Id == id
                select new FormRicDetailResponse
                {
                    Id = r.Id,
                    Judul = r.Judul,

                    Hastag = r.Hastag,
                    AsIsProcessRasciFile = r.AsIsProcessRasciFile,
                    AlternatifSolusi = r.AlternatifSolusi,
                    ToBeProcessBusinessRasciKkiFile = r.ToBeProcessBusinessRasciKkiFile,

                    Permasalahan = r.Permasalahan,
                    DampakMasalah = r.DampakMasalah,
                    FaktorPenyebabMasalah = r.FaktorPenyebabMasalah,
                    SolusiSaatIni = r.SolusiSaatIni,

                    PotensiValueCreation = r.PotensiValueCreation,
                    ExcpectedCompletionTargetFile = r.ExcpectedCompletionTargetFile,
                    HasilSetelahPerbaikan = r.HasilSetelahPerbaikan,

                    Status = r.Status,
                    UpdatedAt = r.UpdatedAt,

                    Reviews = (
                        from rv in _context.ReviewFormRics.AsNoTracking()
                        join u in _context.Users.AsNoTracking()
                            on rv.IdUser equals u.Id
                            into userJoined
                        from user in userJoined.DefaultIfEmpty()
                        where rv.IdFormRic == r.Id
                        orderby rv.CreatedAt
                        select new ReviewRicResponse
                        {
                            Id = rv.Id,
                            Catatan = rv.Catatan,
                            RoleReview = rv.RoleReview.ToString(),
                            UserName = user != null ? user.Name : null,
                            CreatedAt = rv.CreatedAt,
                        }
                    ).ToList(),

                    Histories = (
                        from h in _context.FormRicHistories.AsNoTracking()
                        join u in _context.Users.AsNoTracking()
                            on h.IdEditor equals u.Id
                            into editorJoined
                        from editor in editorJoined.DefaultIfEmpty()
                        where h.IdFormRic == r.Id
                        orderby h.Version descending
                        select new RicHistoryResponse
                        {
                            Version = h.Version,
                            Snapshot = h.SnapshotJson,
                            EditedFields = h.EditedFieldsJson,
                            EditorName = editor != null ? editor.Name : null,
                            CreatedAt = h.CreatedAt,
                        }
                    ).ToList(),
                };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> CreateAsync(FormRic model)
        {
            await _context.FormRics!.AddAsync(model);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(FormRic model)
        {
            _context.FormRics!.Update(model);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.FormRics!.FindAsync(id);
            if (entity is null)
                return false;

            _context.FormRics.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task AddHistoryAsync(FormRicHistory history)
        {
            await _context.FormRicHistories!.AddAsync(history);
            await _context.SaveChangesAsync();
        }

        public async Task AddReviewAsync(ReviewFormRic review)
        {
            await _context.ReviewFormRics!.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> MoveRicToNextStageAsync(FormRic ric, Guid actorId)
        {
            var oldData = await _context
                .FormRics!.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == ric.Id);
            if (oldData is null)
                return false;

            var editedFieldsJson = GetEditedFields(oldData, ric);

            if (editedFieldsJson != null)
            {
                var lastVersion =
                    await _context
                        .FormRicHistories!.Where(x => x.IdFormRic == ric.Id)
                        .MaxAsync(x => (int?)x.Version) ?? 0;

                var history = new FormRicHistory
                {
                    Id = Guid.NewGuid(),
                    IdFormRic = oldData.Id,
                    IdEditor = actorId,
                    Version = lastVersion + 1,
                    SnapshotJson = JsonSerializer.Serialize(oldData),
                    EditedFieldsJson = editedFieldsJson,
                    CreatedAt = DateTime.UtcNow,
                };

                _context.FormRicHistories.Add(history);
            }

            _context.FormRics.Update(ric);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ResubmitAfterRejection(FormRic newData, Guid editorId)
        {
            var oldData = await _context
                .FormRics!.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == newData.Id);
            if (oldData is null)
                return false;

            var lastVersion =
                await _context
                    .FormRicHistories!.Where(x => x.IdFormRic == newData.Id)
                    .MaxAsync(x => (int?)x.Version) ?? 0;

            var history = new FormRicHistory
            {
                Id = Guid.NewGuid(),
                IdFormRic = oldData.Id,
                IdEditor = editorId,
                Version = lastVersion + 1,
                SnapshotJson = JsonSerializer.Serialize(oldData),
                EditedFieldsJson = GetEditedFields(oldData, newData),
                CreatedAt = DateTime.UtcNow,
            };

            _context.FormRicHistories.Add(history);
            _context.FormRics.Update(newData);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<RicListItemResponse>> GetApprovalQueueAsync(
            Guid groupId,
            string role,
            string? q = null,
            int limit = 10
        )
        {
            var query =
                from r in _context.FormRics.AsNoTracking()
                join u in _context.Users.AsNoTracking() on r.IdUser equals u.Id into userJoined
                from user in userJoined.DefaultIfEmpty()
                select new { Ric = r, User = user };

            // 1) tentuin status yg boleh diliat berdasarkan role approver
            StatusRic? targetStatus = role switch
            {
                "User_Manager" => StatusRic.Approval_Manager_User,
                "User_VP" => StatusRic.Approval_VP_User,

                "BR_Manager" => StatusRic.Approval_Manager_BR,

                "SARM_Manager" => StatusRic.Approval_Manager_SARM,
                "SARM_VP" => StatusRic.Approval_VP_SARM,

                "ECS_Manager" => StatusRic.Approval_Manager_ECS,
                "ECS_VP" => StatusRic.Approval_VP_ECS,

                _ => null,
            };

            if (targetStatus is null)
                return new List<RicListItemResponse>();

            query = query.Where(x => x.Ric.Status == targetStatus.Value);

            // 2) scope datanya:
            // - kalau role User_* => cuma RIC dalam group user dia (divisinya)
            // - kalau BR/SARM/ECS => biasanya mereka approve "antrian global" sesuai status,
            //   tapi kalau kamu mau batasi, bisa pakai group khusus mereka.
            var isUserApproval = role is "User_Manager" or "User_VP";
            if (isUserApproval)
            {
                query = query.Where(x => x.Ric.IdGroupUser == groupId);
            }
            else
            {
                // kalau kamu punya konstanta GroupBrId/GroupSarmId/GroupEcsId, bisa aktifin pembatasan ini:
                // if (role.StartsWith("BR_"))   query = query.Where(x => groupId == GroupBrId);
                // if (role.StartsWith("SARM_")) query = query.Where(x => groupId == GroupSarmId);
                // if (role.StartsWith("ECS_"))  query = query.Where(x => groupId == GroupEcsId);

                // default: biarin sesuai status aja
            }

            var items = await query
                .OrderByDescending(x => x.Ric.UpdatedAt)
                .Select(x => new RicListItemResponse
                {
                    Id = x.Ric.Id,
                    Judul = x.Ric.Judul,
                    Permasalahan = x.Ric.Permasalahan,
                    Hastag = x.Ric.Hastag,
                    UserName = x.User != null ? x.User.Name : null,
                    Status = x.Ric.Status.ToString(),
                    UpdatedAt = x.Ric.UpdatedAt,
                })
                .ToListAsync();

            items = ApplySearch(items, q);
            return items.Take(NormalizeLimit(limit)).ToList();
        }

        private static int NormalizeLimit(int limit)
        {
            if (limit <= 0)
                return 10;
            return Math.Min(limit, 100);
        }

        private static List<RicListItemResponse> ApplySearch(
            List<RicListItemResponse> items,
            string? q
        )
        {
            if (string.IsNullOrWhiteSpace(q))
                return items;

            var needle = q.Trim();
            if (needle.StartsWith("#", StringComparison.Ordinal))
                needle = needle.Substring(1);

            return items.Where(x => MatchesSearch(x, needle)).ToList();
        }

        private static bool MatchesSearch(RicListItemResponse item, string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return true;

            if (item.Id.ToString().Contains(q, StringComparison.OrdinalIgnoreCase))
                return true;

            if (!string.IsNullOrWhiteSpace(item.Judul)
                && item.Judul.Contains(q, StringComparison.OrdinalIgnoreCase))
                return true;

            if (!string.IsNullOrWhiteSpace(item.UserName)
                && item.UserName.Contains(q, StringComparison.OrdinalIgnoreCase))
                return true;

            if (item.Hastag?.Any(h =>
                    !string.IsNullOrWhiteSpace(h)
                    && h.Contains(q, StringComparison.OrdinalIgnoreCase)) == true)
                return true;

            return false;
        }

        #region Private Helper
        private static bool ListEquals(List<string>? a, List<string>? b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (a is null || b is null)
                return false;

            if (a.Count != b.Count)
                return false;

            for (var i = 0; i < a.Count; i++)
            {
                if (!string.Equals(a[i], b[i], StringComparison.Ordinal))
                    return false;
            }

            return true;
        }

        private static string? GetEditedFields(FormRic oldVal, FormRic newVal)
        {
            var changes = new Dictionary<string, object?>();

            if (!string.Equals(oldVal.Judul, newVal.Judul, StringComparison.Ordinal))
                changes["judul"] = newVal.Judul;

            if (!ListEquals(oldVal.Hastag, newVal.Hastag))
                changes["hastag"] = newVal.Hastag;

            if (!ListEquals(oldVal.AsIsProcessRasciFile, newVal.AsIsProcessRasciFile))
                changes["asIsProcessRasciFile"] = newVal.AsIsProcessRasciFile;

            if (!string.Equals(oldVal.Permasalahan, newVal.Permasalahan, StringComparison.Ordinal))
                changes["permasalahan"] = newVal.Permasalahan;

            if (
                !string.Equals(oldVal.DampakMasalah, newVal.DampakMasalah, StringComparison.Ordinal)
            )
                changes["dampakMasalah"] = newVal.DampakMasalah;

            if (
                !string.Equals(
                    oldVal.FaktorPenyebabMasalah,
                    newVal.FaktorPenyebabMasalah,
                    StringComparison.Ordinal
                )
            )
                changes["faktorPenyebabMasalah"] = newVal.FaktorPenyebabMasalah;

            if (
                !string.Equals(oldVal.SolusiSaatIni, newVal.SolusiSaatIni, StringComparison.Ordinal)
            )
                changes["solusiSaatIni"] = newVal.SolusiSaatIni;

            if (!ListEquals(oldVal.AlternatifSolusi, newVal.AlternatifSolusi))
                changes["alternatifSolusi"] = newVal.AlternatifSolusi;

            if (
                !ListEquals(
                    oldVal.ToBeProcessBusinessRasciKkiFile,
                    newVal.ToBeProcessBusinessRasciKkiFile
                )
            )
                changes["toBeProcessBusinessRasciKkiFile"] = newVal.ToBeProcessBusinessRasciKkiFile;

            if (
                !string.Equals(
                    oldVal.PotensiValueCreation,
                    newVal.PotensiValueCreation,
                    StringComparison.Ordinal
                )
            )
                changes["potensiValueCreation"] = newVal.PotensiValueCreation;

            if (
                !ListEquals(
                    oldVal.ExcpectedCompletionTargetFile,
                    newVal.ExcpectedCompletionTargetFile
                )
            )
                changes["excpectedCompletionTargetFile"] = newVal.ExcpectedCompletionTargetFile;

            if (
                !string.Equals(
                    oldVal.HasilSetelahPerbaikan,
                    newVal.HasilSetelahPerbaikan,
                    StringComparison.Ordinal
                )
            )
                changes["hasilSetelahPerbaikan"] = newVal.HasilSetelahPerbaikan;

            if (changes.Count == 0)
                return null;

            return JsonSerializer.Serialize(changes, EditedFieldsJsonOptions);
        }

        public async Task<bool> EnsureApprovalsCreatedAsync(Guid ricId)
        {
            // idempotent: kalau sudah ada record approval untuk ric ini, skip
            var exists = await _context.FormRicApprovals!.AnyAsync(a => a.IdFormRic == ricId);
            if (exists)
                return true;

            var now = DateTime.UtcNow;

            var approvals = Enum.GetValues(typeof(RoleApproval))
                .Cast<RoleApproval>()
                .Select(r => new FormRicApproval
                {
                    Id = Guid.NewGuid(),
                    IdFormRic = ricId,
                    IdApprover = Guid.Empty, // belum di-assign
                    Role = r,
                    ApprovalStatus = ApprovalStatus.Pending, // default Pending
                    ApprovalDate = null,
                    CreatedAt = now,
                })
                .ToList();

            await _context.FormRicApprovals!.AddRangeAsync(approvals);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkApprovalApprovedAsync(
            Guid ricId,
            RoleApproval role,
            Guid approverId
        )
        {
            var pending = await _context.FormRicApprovals!.FirstOrDefaultAsync(a =>
                a.IdFormRic == ricId && a.Role == role && a.ApprovalStatus == ApprovalStatus.Pending
            );

            if (pending is null)
            {
                var alreadyApproved = await _context.FormRicApprovals!.AnyAsync(a =>
                    a.IdFormRic == ricId
                    && a.Role == role
                    && a.ApprovalStatus == ApprovalStatus.Approved
                );

                return alreadyApproved;
            }

            pending.IdApprover = approverId;
            pending.ApprovalStatus = ApprovalStatus.Approved;
            pending.ApprovalDate = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }

        #endregion Private helper
    }
}
