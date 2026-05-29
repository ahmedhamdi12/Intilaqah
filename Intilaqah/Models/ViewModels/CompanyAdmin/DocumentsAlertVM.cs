namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class DocumentsAlertVM
    {
        public int TotalDocuments    { get; set; }
        public int ExpiredCount      { get; set; }
        public int Expiring30Count   { get; set; }
        public int Expiring60Count   { get; set; }
        public int Expiring90Count   { get; set; }
        public int ValidCount        { get; set; }

        public List<DocumentListItemVM> ExpiredDocs    { get; set; } = new();
        public List<DocumentListItemVM> Expiring30Docs { get; set; } = new();
        public List<DocumentListItemVM> Expiring60Docs { get; set; } = new();
        public List<DocumentListItemVM> Expiring90Docs { get; set; } = new();
        public List<DocumentListItemVM> CompanyDocs    { get; set; } = new();
        public List<DocumentListItemVM> EmployeeDocs   { get; set; } = new();
    }
}
