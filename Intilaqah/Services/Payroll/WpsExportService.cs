using ClosedXML.Excel;
using Intilaqah.Models;
using Intilaqah.UnitOfWork;

namespace Intilaqah.Services.Payroll
{
    public class WpsExportService : IWpsExportService
    {
        private readonly IUnitOfWork _uow;

        public WpsExportService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<byte[]> ExportWpsExcelAsync(Guid payrollRunId, string exportedBy)
        {
            var run = await _uow.Payroll.GetWithPaySlipsAsync(payrollRunId)
                ?? throw new KeyNotFoundException("مسير الرواتب غير موجود");

            var employeeIds = run.PaySlips.Select(ps => ps.EmployeeId).ToList();
            var bankAccounts = (await _uow.EmployeeBankAccounts.FindAsync(b => b.IsActive && employeeIds.Contains(b.EmployeeId)))
                .ToDictionary(b => b.EmployeeId);

            // Validation: Mudad requires IBAN
            var missingIbanEmployees = new List<string>();
            foreach (var slip in run.PaySlips)
            {
                if (!bankAccounts.TryGetValue(slip.EmployeeId, out var acc) || string.IsNullOrWhiteSpace(acc.Iban))
                {
                    missingIbanEmployees.Add(slip.EmployeeNameAr);
                }
            }

            if (missingIbanEmployees.Any())
            {
                var names = string.Join(", ", missingIbanEmployees);
                throw new InvalidOperationException($"لا يمكن تصدير ملف حماية الأجور (WPS). بيانات البنك (IBAN) مفقودة للموظفين: {names}");
            }

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("WPS");

            // Mudad standard columns (example of SIF standard)
            // 1. Employee QID / Iqama
            // 2. Employee Name
            // 3. Bank Code / Routing
            // 4. Account Number / IBAN
            // 5. Basic Wage
            // 6. Housing Allowance
            // 7. Other Earnings
            // 8. Deductions
            // 9. Net Salary
            
            // Standard headers (Mudad accepts english standard headers)
            var headers = new[]
            {
                "Employee QID", "Employee Name", "Bank Code", "Account Number",
                "Basic Wage", "Housing Allowance", "Other Earnings", "Deductions", "Net Salary"
            };

            for (int c = 0; c < headers.Length; c++)
                ws.Cell(1, c + 1).Value = headers[c];

            var headerRange = ws.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3c5e");
            headerRange.Style.Font.FontColor = XLColor.White;

            int row = 2;
            foreach (var slip in run.PaySlips.OrderBy(p => p.EmployeeCode))
            {
                var acc = bankAccounts[slip.EmployeeId];
                
                ws.Cell(row, 1).Value = slip.NationalId;
                ws.Cell(row, 2).Value = slip.EmployeeNameEn; // English name is usually preferred for WPS
                ws.Cell(row, 3).Value = acc.BankName; // Or bank routing code
                ws.Cell(row, 4).Value = acc.Iban;
                ws.Cell(row, 5).Value = (double)slip.BasicSalary;
                ws.Cell(row, 6).Value = (double)slip.HousingAllowance;
                ws.Cell(row, 7).Value = (double)(slip.TransportAllowance + slip.OtherAllowances + slip.OvertimeAmount);
                ws.Cell(row, 8).Value = (double)slip.TotalDeductions;
                ws.Cell(row, 9).Value = (double)slip.NetSalary;
                
                row++;
            }

            ws.Columns().AdjustToContents();

            // Mark as exported
            if (run.Status == PayrollStatus.Approved)
            {
                run.Status = PayrollStatus.Exported;
            }
            run.ExportedAt = DateTime.UtcNow;
            run.ExportedBy = exportedBy;
            run.UpdatedBy = exportedBy;
            run.UpdatedAt = DateTime.UtcNow;
            
            _uow.Payroll.Update(run);
            await _uow.SaveChangesAsync();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
