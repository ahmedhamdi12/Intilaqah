using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext context, ITenantResolver tenantResolver) : base(context, tenantResolver) { }

        public async Task<int> CountSaudiAsync()
        => await _dbSet.CountAsync(e => e.Nationality == NationalityType.Saudi && e.IsActive);

        public async Task<int> CountNonSaudiAsync()
            => await _dbSet.CountAsync(e => e.Nationality == NationalityType.NonSaudi && e.IsActive);

        public async Task<decimal> GetSaudizationPercentageAsync()
        {
            var total = await _dbSet.CountAsync(e => e.IsActive);
            if (total == 0) return 0;
            var saudi = await CountSaudiAsync();
            return Math.Round((decimal)saudi / total * 100, 2);
        }
    }
}
