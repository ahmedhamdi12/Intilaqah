using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public class ShiftAssignment : BaseEntity
    {
        public Guid     EmployeeId  { get; set; }
        public Guid     ShiftId     { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public bool     IsActive    { get; set; } = true;

        // Navigation
        public Employee Employee { get; set; } = null!;
        public Shift    Shift    { get; set; } = null!;
    }
}
