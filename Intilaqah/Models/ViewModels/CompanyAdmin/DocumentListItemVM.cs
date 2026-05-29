using Intilaqah.Models;

namespace Intilaqah.Models.ViewModels.CompanyAdmin
{
    public class DocumentListItemVM
    {
        public Guid               Id           { get; set; }
        public Guid               EntityId     { get; set; }
        public DocumentEntityType EntityType   { get; set; }
        public string             EntityName   { get; set; } = "";
        public string             DocType      { get; set; } = "";
        public string?            FilePath     { get; set; }
        public DateTime?          ExpiryDate   { get; set; }
        public int?               DaysRemaining { get; set; }
        public string             UrgencyClass { get; set; } = "";
        public string             UrgencyLabel { get; set; } = "";
        public bool               HasFile      { get; set; }
    }
}
