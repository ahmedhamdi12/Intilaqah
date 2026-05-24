using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public enum DocumentEntityType { Company, Employee }
    public class Document : BaseEntity
    {
        public DocumentEntityType EntityType { get; set; }
        public Guid EntityId { get; set; }
        public string DocType { get; set; } = "";
        public string? FilePath { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public Tenant Tenant { get; set; } = null!;
    }
}
