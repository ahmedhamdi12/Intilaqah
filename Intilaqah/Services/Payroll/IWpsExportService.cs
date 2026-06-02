using Intilaqah.Models;

namespace Intilaqah.Services.Payroll
{
    public interface IWpsExportService
    {
        Task<byte[]> ExportWpsExcelAsync(Guid payrollRunId, string exportedBy);
    }
}
