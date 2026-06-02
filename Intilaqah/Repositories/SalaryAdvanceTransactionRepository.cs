using Intilaqah.Data;
using Intilaqah.Models;
using Intilaqah.Repositories.Interfaces;
using Intilaqah.Services;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Repositories
{
    public class SalaryAdvanceTransactionRepository : GenericRepository<SalaryAdvanceTransaction>, ISalaryAdvanceTransactionRepository
    {
        public SalaryAdvanceTransactionRepository(ApplicationDbContext context, ITenantResolver tenantResolver)
            : base(context, tenantResolver) { }

        public async Task<IEnumerable<SalaryAdvanceTransaction>> GetByAdvanceIdAsync(Guid salaryAdvanceId)
            => await _dbSet
                .Where(t => t.SalaryAdvanceId == salaryAdvanceId)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync();
    }
}
