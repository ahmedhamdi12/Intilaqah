namespace Intilaqah.Models
{
    public class AuditLog
    {
        public Guid      Id         { get; set; } = Guid.NewGuid();
        public Guid?     TenantId   { get; set; }   // null for SuperAdmin
        public string?   UserId     { get; set; }
        public string?   UserName   { get; set; }
        public string    Action     { get; set; } = "";  // "Create","Update","Delete"
        public string    EntityName { get; set; } = "";  // e.g. "Employee"
        public string?   EntityId   { get; set; }
        public string?   OldValues  { get; set; }   // JSON
        public string?   NewValues  { get; set; }   // JSON
        public string?   IpAddress  { get; set; }
        public DateTime  CreatedAt  { get; set; } = DateTime.UtcNow;
    }
}
