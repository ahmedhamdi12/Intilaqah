namespace Intilaqah.Models.ViewModels.Employee
{
    public class PayslipListItemVM
    {
        public Guid     PayrollRunId  { get; set; }
        public int      Month         { get; set; }
        public int      Year          { get; set; }
        public string   MonthName     { get; set; } = "";
        public decimal  GrossSalary   { get; set; }
        public decimal  TotalDeductions { get; set; }
        public decimal  NetSalary     { get; set; }
        public int      PresentDays   { get; set; }
        public int      AbsentDays    { get; set; }
    }
}
