namespace Intilaqah.Models
{
    public enum NotificationType
    {
        DocumentExpiry,    // وثيقة تنتهي قريباً
        PayrollReady,      // مسير رواتب جاهز
        LeaveRequest,      // طلب إجازة
        ViolationAdded,    // مخالفة مضافة
        ContractExpiry,    // عقد موظف ينتهي
        General            // عام
    }

    public class Notification
    {
        public Guid             Id         { get; set; } = Guid.NewGuid();
        public string           UserId     { get; set; } = "";
        public Guid?            TenantId   { get; set; }
        public NotificationType Type       { get; set; }
        public string           Title      { get; set; } = "";
        public string           Message    { get; set; } = "";
        public string?          ActionUrl  { get; set; }  // link to relevant page
        public bool             IsRead     { get; set; } = false;
        public DateTime         CreatedAt  { get; set; } = DateTime.UtcNow;
        public DateTime?        ReadAt     { get; set; }
    }
}
