namespace Intilaqah.Models
{
    public enum DocumentEntityType { Company, Employee }
    public class Document
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        public DocumentEntityType EntityType { get; set; }
        public Guid EntityId { get; set; }
        public string DocType { get; set; } = ""; // e.g. "iqama", "passport"
        public string? FilePath { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Tenant Tenant { get; set; } = null!;
    }
}
