using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Core.ViewModels;
using Microsoft.EntityFrameworkCore;
using OnePro.API.Interfaces;

namespace OnePro.API.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly OneProDbContext _context;

        public GroupRepository(OneProDbContext context)
        {
            _context = context;
        }

        public async Task<GroupWithMembersResponse?> GetGroupWithMembersAsync(Guid groupId)
        {
            // Ambil group
            var group = await _context
                .Groups!.AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return null;

            // Ambil member dari group itu
            var members = await _context
                .Users!.Where(u => u.IdGroup == groupId)
                .AsNoTracking()
                .ToListAsync();

            // Build response
            return new GroupWithMembersResponse
            {
                Id = group.Id,
                NamaDivisi = group.NamaDivisi,
                NamaPerusahaan = group.NamaPerusahaan,
                Members = members
                    .Select(u => new Members
                    {
                        Name = u.Name,
                        Email = u.Email,
                        Position = u.Position,
                        Role = (int)u.Role,
                    })
                    .ToList(),
            };
        }
    }
}
