using ClosedXML.Excel;
using Intilaqah.Models;
using Intilaqah.UnitOfWork;

namespace Intilaqah.Services.Payroll
{
    public class PayrollReportExportService : IPayrollReportExportService
    {
        private readonly IUnitOfWork _uow;

        public PayrollReportExportService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<byte[]> ExportInternalReportAsync(Guid payrollRunId)
        {
            var run = await _uow.Payroll.GetWithPaySlipsAsync(payrollRunId)
                ?? throw new KeyNotFoundException("مسير الرواتب غير موجود");

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("التقرير الداخلي للرواتب");
            ws.RightToLeft = true;

            // Headers (Row 1)
            var headers = new[]
            {
                "رقم الموظف", "الاسم بالعربية", "رقم الهوية",
                "الراتب الأساسي", "بدل السكن", "بدل النقل",
                "بدلات أخرى", "بدل الإضافي",
                "خصم التأخر", "خصم الغياب", "خصم المخالفات",
                "خصم السلف", "صافي الراتب"
            };

            for (int c = 0; c < headers.Length; c++)
                ws.Cell(1, c + 1).Value = headers[c];

            // Style header row
            var headerRange = ws.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3c5e");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Data rows
            int row = 2;
            foreach (var slip in run.PaySlips.OrderBy(p => p.EmployeeCode))
            {
                ws.Cell(row, 1).Value  = slip.EmployeeCode;
                ws.Cell(row, 2).Value  = slip.EmployeeNameAr;
                ws.Cell(row, 3).Value  = slip.NationalId;
                ws.Cell(row, 4).Value  = (double)slip.BasicSalary;
                ws.Cell(row, 5).Value  = (double)slip.HousingAllowance;
                ws.Cell(row, 6).Value  = (double)slip.TransportAllowance;
                ws.Cell(row, 7).Value  = (double)slip.OtherAllowances;
                ws.Cell(row, 8).Value  = (double)slip.OvertimeAmount;
                ws.Cell(row, 9).Value  = (double)slip.LateDeduction;
                ws.Cell(row, 10).Value = (double)slip.AbsenceDeduction;
                ws.Cell(row, 11).Value = (double)slip.ViolationDeduction;
                ws.Cell(row, 12).Value = (double)slip.AdvanceDeduction;
                ws.Cell(row, 13).Value = (double)slip.NetSalary;

                // Highlight net salary column
                ws.Cell(row, 13).Style.Font.Bold = true;
                if (row % 2 == 0)
                    ws.Range(row, 1, row, headers.Length)
                        .Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fb");

                row++;
            }

            ws.Columns().AdjustToContents();

            // Total row
            ws.Cell(row, 1).Value = "الإجمالي";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 13).Value = (double)run.TotalNet;
            ws.Cell(row, 13).Style.Font.Bold = true;

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
