using Intilaqah.Models;

namespace Intilaqah.Repositories.Interfaces
{
    public interface IPermissionRepository
    {
        Task<IEnumerable<Permission>> GetByRoleAsync(string roleId);
        Task<bool> HasPermissionAsync(string roleId, string permissionName);
        Task<IEnumerable<Permission>> GetAllAsync();
    }
}
