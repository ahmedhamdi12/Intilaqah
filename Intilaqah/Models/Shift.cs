using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public enum ShiftType { Fixed, Variable, Night, Seasonal }

    public class Shift : BaseEntity
    {
        public string    Name        { get; set; } = "";
        public ShiftType ShiftType   { get; set; }
        public TimeOnly  StartTime   { get; set; }
        public TimeOnly  EndTime     { get; set; }
        public int       GraceMinutes { get; set; } = 5;
        public bool      IsActive    { get; set; } = true;

        // Navigation
        public ICollection<ShiftAssignment> ShiftAssignments { get; set; } = [];
    }
}
