namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class AttendanceImportResultVM
    {
        public int    ImportedCount { get; set; }
        public int    SkippedCount  { get; set; }
        public int    ErrorCount    { get; set; }
        public string Message       { get; set; } = "";
        public List<string> Errors  { get; set; } = new();
    }
}
