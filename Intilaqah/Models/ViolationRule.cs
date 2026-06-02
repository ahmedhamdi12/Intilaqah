using Intilaqah.Models.Base;

namespace Intilaqah.Models
{
    public enum ViolationSeverity { Minor, Moderate, Serious, Severe }

    public class ViolationRule : BaseEntity
    {
        public int               RuleNumber   { get; set; }  // 1-36
        public string            Title        { get; set; } = "";
        public string?           Description  { get; set; }
        public ViolationSeverity Severity     { get; set; }
        public decimal           DeductionAmount { get; set; }
        public bool              IsActive     { get; set; } = true;

        public ICollection<ViolationRecord> ViolationRecords { get; set; } = [];
    }
}
