using Core.Models;
using Core.Models.Entities;
using Core.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OnePro.API.Interfaces;

namespace OnePro.API.Repositories
{
    public class UndanganFormRicRepository(OneProDbContext context) : IUndanganFormRicRepository
    {
        private readonly OneProDbContext _context = context;

        public async Task<List<UndanganFormRicResponse>> GetAllAsync()
        {
            var data = await _context.UndanganFormRicResponses!
                .FromSqlRaw("EXEC SP_UndanganFormRics_Get")
                .ToListAsync();

            return data;
        }

        public async Task<UndanganFormRicResponse?> GetByIdAsync(Guid id)
        {
            var pId = new SqlParameter("@Id", id);

            var data = await _context.UndanganFormRicResponses!
                .FromSqlRaw("EXEC SP_UndanganFormRics_GetById @Id", pId)
                .ToListAsync();

            return data.FirstOrDefault();
        }

        public async Task<bool> CreateAsync(UndanganFormRic model)
        {
            var parameters = new[]
            {
                new SqlParameter("@Id", model.Id),
                new SqlParameter("@IdBr", model.IdBr),
                new SqlParameter("@IdUser", model.IdUser),
                new SqlParameter("@IdGroupUser", model.IdGroupUser),
                new SqlParameter("@EmailUser", model.EmailUser),
                new SqlParameter("@Subject", model.Subject),
                new SqlParameter("@Content", model.Content),
                new SqlParameter("@Link", (object?)model.Link ?? string.Empty)
            };

            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_UndanganFormRics_Create @Id,@IdBr,@IdUser,@IdGroupUser,@EmailUser,@Subject,@Content,@Link",
                parameters
            );

            return result > 0;
        }

        public async Task<bool> UpdateAsync(UndanganFormRic model)
        {
            var parameters = new[]
            {
                new SqlParameter("@Id", model.Id),
                new SqlParameter("@Subject", model.Subject),
                new SqlParameter("@Content", model.Content),
                new SqlParameter("@Link", (object?)model.Link ?? string.Empty)
            };

            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_UndanganFormRics_Update @Id,@Subject,@Content,@Link",
                parameters
            );

            return result > 0;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var pId = new SqlParameter("@Id", id);

            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_UndanganFormRics_Delete @Id",
                pId
            );

            return result > 0;
        }
    }
}
