namespace Intilaqah.Models.Integration
{
    public enum IntegrationProvider
    {
        Qiwa,    // منصة قوى
        Mudad,   // منصة مدد
        Gosi,    // التأمينات الاجتماعية (مستقبلاً)
        Elm,     // شركة علم (مستقبلاً)
    }

    public enum IntegrationStatus
    {
        Success,
        Failed,
        Pending,
        Retrying,
        Cancelled,
    }
}
