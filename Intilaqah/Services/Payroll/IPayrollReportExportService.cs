using Intilaqah.Models;

namespace Intilaqah.Services.Payroll
{
    public interface IPayrollReportExportService
    {
        Task<byte[]> ExportInternalReportAsync(Guid payrollRunId);
    }
}
