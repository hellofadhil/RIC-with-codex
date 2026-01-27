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
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                        Position = u.Position,
                        Role = (int)u.Role,
                    })
                    .ToList(),
            };
        }

        public async Task<Core.Models.Entities.User?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users!.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<Core.Models.Entities.User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users!.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> CreateGroupAsync(Core.Models.Entities.Group group)
        {
            _context.Groups!.Add(group);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddMemberAsync(Core.Models.Entities.User user)
        {
            _context.Users!.Add(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateMemberRoleAsync(Core.Models.Entities.User user, Core.Models.Enums.Role newRole)
        {
            user.Role = newRole;
            _context.Users!.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateMemberAsync(Core.Models.Entities.User user)
        {
            _context.Users!.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteMemberAsync(Core.Models.Entities.User user)
        {
            user.IdGroup = null;
            _context.Users!.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
