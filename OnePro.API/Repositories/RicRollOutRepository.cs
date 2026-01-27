using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Core.Models;
using Core.Models.Entities;
using Core.Models.Enums;
using Core.RequestModels.RicRollOut;
using Microsoft.EntityFrameworkCore;
using OnePro.API.Interfaces;

namespace OnePro.API.Repositories
{
    public class RicRollOutRepository : IRicRollOutRepository
    {
        private readonly OneProDbContext _context;

        // BR group
        private static readonly Guid GroupBrId = Guid.Parse("20000000-0000-0000-0000-000000000002");

        private static readonly JsonSerializerOptions EditedFieldsJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public RicRollOutRepository(OneProDbContext context)
        {
            _context = context;
        }

        public async Task<List<RicRollOutListItemResponse>> GetAllByGroupAsync(Guid groupId)
        {
            var query =
                from r in _context.FormRicRollOuts!.AsNoTracking()
                join u in _context.Users!.AsNoTracking() on r.IdUser equals u.Id into userJoined
                from user in userJoined.DefaultIfEmpty()
                select new { RollOut = r, User = user };

            // BR reviewer mode
            if (groupId == GroupBrId)
            {
                query = query.Where(x =>
                    x.RollOut.Status == StatusRicRollOut.Submitted_To_BR
                    || x.RollOut.Status == StatusRicRollOut.Review_BR
                );
            }
            else
            {
                query = query.Where(x => x.RollOut.IdGroupUser == groupId);
            }

            return await query
                .Select(x => new RicRollOutListItemResponse
                {
                    Id = x.RollOut.Id,
                    Entitas = x.RollOut.Entitas,
                    JudulAplikasi = x.RollOut.JudulAplikasi,
                    UserName = x.User != null ? x.User.Name : null,
                    Status = x.RollOut.Status.ToString(),
                    UpdatedAt = x.RollOut.UpdatedAt,
                })
                .ToListAsync();
        }

        public async Task<FormRicRollOut?> GetByIdAsync(Guid id)
        {
            return await _context
                .FormRicRollOuts!.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<FormRicRollOutDetailResponse?> GetDetailByIdAsync(Guid id)
        {
            var query =
                from r in _context.FormRicRollOuts!.AsNoTracking()
                join u in _context.Users!.AsNoTracking() on r.IdUser equals u.Id into userJoined
                from user in userJoined.DefaultIfEmpty()
                join g in _context.Groups!.AsNoTracking()
                    on r.IdGroupUser equals g.Id
                    into groupJoined
                from grp in groupJoined.DefaultIfEmpty()
                where r.Id == id
                select new FormRicRollOutDetailResponse
                {
                    Id = r.Id,

                    // OWNER
                    IdUser = r.IdUser,
                    UserName = user != null ? user.Name : string.Empty,
                    IdGroupUser = r.IdGroupUser,
                    GroupName = grp != null ? grp.NamaDivisi : string.Empty,

                    // MAIN DATA
                    Entitas = r.Entitas,
                    JudulAplikasi = r.JudulAplikasi,
                    Hashtag = r.Hashtag,

                    // FILES
                    CompareWithAsIsHoldingProcessFiles = r.CompareWithAsIsHoldingProcessFiles,
                    StkAsIsToBeFiles = r.StkAsIsToBeFiles,

                    // CHECKLIST
                    IsJoinedDomainAdPertamina = r.IsJoinedDomainAdPertamina,
                    IsUsingErpPertamina = r.IsUsingErpPertamina,
                    IsImplementedRequiredActivation = r.IsImplementedRequiredActivation,
                    HasDataCenterConnection = r.HasDataCenterConnection,
                    HasRequiredResource = r.HasRequiredResource,

                    // STATUS
                    Status = r.Status,

                    // META
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> CreateAsync(FormRicRollOut model)
        {
            await _context.FormRicRollOuts!.AddAsync(model);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(FormRicRollOut model)
        {
            _context.FormRicRollOuts!.Update(model);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<FormRicRollOutDetailResponse?> GetDetailAsync(Guid id)
        {
            var e = await _context
                .FormRicRollOuts.AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Group)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (e == null)
                return null;

            return new FormRicRollOutDetailResponse
            {
                Id = e.Id,

                IdUser = e.IdUser,
                UserName = e.User?.Name, // kalau User entity beda, sesuaikan (misal FullName)

                IdGroupUser = e.IdGroupUser,
                // GroupName = e.Group?.GroupName ?? e.Group?.NamaGroup, // ganti sesuai property Group lu
                // kalau gak tau propertynya, paling aman:
                // GroupName = null,

                Entitas = e.Entitas,
                JudulAplikasi = e.JudulAplikasi,

                Hashtag = e.Hashtag ?? new List<string>(),

                CompareWithAsIsHoldingProcessFiles =
                    e.CompareWithAsIsHoldingProcessFiles ?? new List<string>(),
                StkAsIsToBeFiles = e.StkAsIsToBeFiles ?? new List<string>(),

                IsJoinedDomainAdPertamina = e.IsJoinedDomainAdPertamina,
                IsUsingErpPertamina = e.IsUsingErpPertamina,
                IsImplementedRequiredActivation = e.IsImplementedRequiredActivation,
                HasDataCenterConnection = e.HasDataCenterConnection,
                HasRequiredResource = e.HasRequiredResource,

                Status = e.Status,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,

                Reviews = new List<RicRollOutReviewResponse>(),
                Histories = new List<RicRollOutHistoryResponse>(),
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.FormRicRollOuts!.FindAsync(id);
            if (entity is null)
                return false;

            _context.FormRicRollOuts.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task AddHistoryAsync(FormRicRollOutHistory history)
        {
            await _context.FormRicRollOutHistories!.AddAsync(history);
            await _context.SaveChangesAsync();
        }

        public async Task AddReviewAsync(ReviewFormRicRollOut review)
        {
            await _context.ReviewFormRicRollOuts!.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> MoveRollOutToNextStageAsync(FormRicRollOut rollOut, Guid actorId)
        {
            var oldData = await _context
                .FormRicRollOuts!.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == rollOut.Id);

            if (oldData is null)
                return false;

            var editedFieldsJson = GetEditedFields(oldData, rollOut);

            if (editedFieldsJson != null)
            {
                var lastVersion =
                    await _context
                        .FormRicRollOutHistories!.Where(x => x.IdFormRicRollOut == rollOut.Id)
                        .MaxAsync(x => (int?)x.Version) ?? 0;

                var history = new FormRicRollOutHistory
                {
                    Id = Guid.NewGuid(),
                    IdFormRicRollOut = oldData.Id,
                    IdEditor = actorId,
                    Version = lastVersion + 1,
                    SnapshotJson = JsonSerializer.Serialize(oldData),
                    EditedFieldsJson = editedFieldsJson,
                    CreatedAt = DateTime.UtcNow,
                };

                _context.FormRicRollOutHistories.Add(history);
            }

            _context.FormRicRollOuts.Update(rollOut);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ResubmitAfterRejectionAsync(FormRicRollOut newData, Guid editorId)
        {
            var oldData = await _context
                .FormRicRollOuts!.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == newData.Id);

            if (oldData is null)
                return false;

            var lastVersion =
                await _context
                    .FormRicRollOutHistories!.Where(x => x.IdFormRicRollOut == newData.Id)
                    .MaxAsync(x => (int?)x.Version) ?? 0;

            var history = new FormRicRollOutHistory
            {
                Id = Guid.NewGuid(),
                IdFormRicRollOut = oldData.Id,
                IdEditor = editorId,
                Version = lastVersion + 1,
                SnapshotJson = JsonSerializer.Serialize(oldData),
                EditedFieldsJson = GetEditedFields(oldData, newData),
                CreatedAt = DateTime.UtcNow,
            };

            _context.FormRicRollOutHistories.Add(history);
            _context.FormRicRollOuts.Update(newData);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> EnsureApprovalsCreatedAsync(Guid rollOutId)
        {
            var exists = await _context.FormRicRollOutApprovals!.AnyAsync(a =>
                a.IdFormRicRollOut == rollOutId
            );

            if (exists)
                return true;

            var approval = new FormRicRollOutApproval
            {
                Id = Guid.NewGuid(),
                IdFormRicRollOut = rollOutId,
                IdApprover = Guid.Empty,
                Role = RoleApproval.BR_Manager,
                ApprovalStatus = ApprovalStatus.Pending,
                ApprovalDate = null,
                CreatedAt = DateTime.UtcNow,
            };

            await _context.FormRicRollOutApprovals!.AddAsync(approval);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkApprovalApprovedAsync(
            Guid rollOutId,
            RoleApproval role,
            Guid approverId
        )
        {
            var pending = await _context.FormRicRollOutApprovals!.FirstOrDefaultAsync(a =>
                a.IdFormRicRollOut == rollOutId
                && a.Role == role
                && a.ApprovalStatus == ApprovalStatus.Pending
            );

            if (pending is null)
                return false;

            pending.IdApprover = approverId;
            pending.ApprovalStatus = ApprovalStatus.Approved;
            pending.ApprovalDate = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }

        // ===================== HELPERS =====================

        private static bool ListEquals(List<string>? a, List<string>? b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;
            if (a.Count != b.Count)
                return false;

            for (var i = 0; i < a.Count; i++)
                if (!string.Equals(a[i], b[i], StringComparison.Ordinal))
                    return false;

            return true;
        }

        private static string? GetEditedFields(FormRicRollOut oldVal, FormRicRollOut newVal)
        {
            var changes = new Dictionary<string, object?>();

            if (!string.Equals(oldVal.Entitas, newVal.Entitas, StringComparison.Ordinal))
                changes["entitas"] = newVal.Entitas;

            if (
                !string.Equals(oldVal.JudulAplikasi, newVal.JudulAplikasi, StringComparison.Ordinal)
            )
                changes["judulAplikasi"] = newVal.JudulAplikasi;

            if (!ListEquals(oldVal.Hashtag, newVal.Hashtag))
                changes["hashtag"] = newVal.Hashtag;

            if (
                !ListEquals(
                    oldVal.CompareWithAsIsHoldingProcessFiles,
                    newVal.CompareWithAsIsHoldingProcessFiles
                )
            )
                changes["compareWithAsIsHoldingProcessFiles"] =
                    newVal.CompareWithAsIsHoldingProcessFiles;

            if (!ListEquals(oldVal.StkAsIsToBeFiles, newVal.StkAsIsToBeFiles))
                changes["stkAsIsToBeFiles"] = newVal.StkAsIsToBeFiles;

            if (oldVal.IsJoinedDomainAdPertamina != newVal.IsJoinedDomainAdPertamina)
                changes["isJoinedDomainAdPertamina"] = newVal.IsJoinedDomainAdPertamina;

            if (oldVal.IsUsingErpPertamina != newVal.IsUsingErpPertamina)
                changes["isUsingErpPertamina"] = newVal.IsUsingErpPertamina;

            if (oldVal.IsImplementedRequiredActivation != newVal.IsImplementedRequiredActivation)
                changes["isImplementedRequiredActivation"] = newVal.IsImplementedRequiredActivation;

            if (oldVal.HasDataCenterConnection != newVal.HasDataCenterConnection)
                changes["hasDataCenterConnection"] = newVal.HasDataCenterConnection;

            if (oldVal.HasRequiredResource != newVal.HasRequiredResource)
                changes["hasRequiredResource"] = newVal.HasRequiredResource;

            if (changes.Count == 0)
                return null;

            return JsonSerializer.Serialize(changes, EditedFieldsJsonOptions);
        }
    }
}
