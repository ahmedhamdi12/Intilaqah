using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly ApplicationDbContext _context;

        public PermissionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Permission>> GetByRoleAsync(string roleId)
            => await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.Permission)
                .ToListAsync();

        public async Task<bool> HasPermissionAsync(string roleId, string permissionName)
            => await _context.RolePermissions
                .AnyAsync(rp => rp.RoleId == roleId
                             && rp.Permission.Name == permissionName);

        public async Task<IEnumerable<Permission>> GetAllAsync()
            => await _context.Permissions.ToListAsync();
    }
}
