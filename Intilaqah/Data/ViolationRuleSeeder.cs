using Intilaqah.Models;
using Microsoft.EntityFrameworkCore;

namespace Intilaqah.Data
{
    public static class ViolationRuleSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Set<ViolationRule>().AnyAsync())
            {
                return;
            }

            // Standard Saudi Labor Law Violation Rules (36 Items)
            var rules = new List<ViolationRule>
            {
                new() { RuleNumber = 1, Title = "التأخر عن العمل بدون إذن (1-15 دقيقة)", Severity = ViolationSeverity.Minor, DeductionAmount = 0.05m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 2, Title = "التأخر عن العمل بدون إذن (16-30 دقيقة)", Severity = ViolationSeverity.Minor, DeductionAmount = 0.10m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 3, Title = "التأخر عن العمل بدون إذن (31-60 دقيقة)", Severity = ViolationSeverity.Minor, DeductionAmount = 0.25m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 4, Title = "التأخر عن العمل بدون إذن (أكثر من ساعة)", Severity = ViolationSeverity.Moderate, DeductionAmount = 0.50m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 5, Title = "الغياب عن العمل ليوم واحد بدون عذر", Severity = ViolationSeverity.Moderate, DeductionAmount = 1.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 6, Title = "الغياب عن العمل ليومين بدون عذر", Severity = ViolationSeverity.Moderate, DeductionAmount = 2.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 7, Title = "الغياب عن العمل لثلاثة أيام بدون عذر", Severity = ViolationSeverity.Serious, DeductionAmount = 3.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 8, Title = "الغياب عن العمل لأربعة أيام بدون عذر", Severity = ViolationSeverity.Serious, DeductionAmount = 4.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 9, Title = "الانصراف قبل المواعيد بدون إذن (أقل من 15 دقيقة)", Severity = ViolationSeverity.Minor, DeductionAmount = 0.10m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 10, Title = "الانصراف قبل المواعيد بدون إذن (أكثر من 15 دقيقة)", Severity = ViolationSeverity.Moderate, DeductionAmount = 0.25m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 11, Title = "التمارض وتصنع المرض للتهرب من العمل", Severity = ViolationSeverity.Moderate, DeductionAmount = 0.50m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 12, Title = "عدم التوقيع في سجل الحضور والانصراف", Severity = ViolationSeverity.Minor, DeductionAmount = 0.10m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 13, Title = "توقيع الموظف لموظف آخر أو العكس", Severity = ViolationSeverity.Serious, DeductionAmount = 5.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 14, Title = "عدم ارتداء الزي الرسمي أثناء العمل", Severity = ViolationSeverity.Minor, DeductionAmount = 0.25m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 15, Title = "النوم أثناء ساعات العمل", Severity = ViolationSeverity.Serious, DeductionAmount = 1.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 16, Title = "استقبال زوار من خارج العمل لأمور شخصية", Severity = ViolationSeverity.Minor, DeductionAmount = 0.25m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 17, Title = "التدخين في الأماكن غير المخصصة لذلك", Severity = ViolationSeverity.Moderate, DeductionAmount = 0.50m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 18, Title = "الأكل والشرب في أماكن غير مخصصة", Severity = ViolationSeverity.Minor, DeductionAmount = 0.10m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 19, Title = "استخدام الهاتف الشخصي بشكل مفرط", Severity = ViolationSeverity.Minor, DeductionAmount = 0.25m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 20, Title = "استخدام أجهزة الشركة لأغراض شخصية", Severity = ViolationSeverity.Moderate, DeductionAmount = 0.50m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 21, Title = "التسبب في إتلاف معدات العمل بإهمال", Severity = ViolationSeverity.Serious, DeductionAmount = 5.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 22, Title = "العبث في ممتلكات المنشأة", Severity = ViolationSeverity.Serious, DeductionAmount = 5.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 23, Title = "رفض العمل الإضافي المبرر", Severity = ViolationSeverity.Moderate, DeductionAmount = 1.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 24, Title = "عدم الالتزام بتعليمات الرؤساء", Severity = ViolationSeverity.Serious, DeductionAmount = 2.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 25, Title = "تجاوز الرؤساء المباشرين برفع الشكاوى", Severity = ViolationSeverity.Minor, DeductionAmount = 0.25m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 26, Title = "إثارة الشغب والمنازعات داخل العمل", Severity = ViolationSeverity.Serious, DeductionAmount = 5.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 27, Title = "التلفظ بألفاظ غير لائقة مع الزملاء", Severity = ViolationSeverity.Moderate, DeductionAmount = 1.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 28, Title = "التلفظ بألفاظ غير لائقة مع الرؤساء", Severity = ViolationSeverity.Serious, DeductionAmount = 3.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 29, Title = "عدم المحافظة على أسرار العمل", Severity = ViolationSeverity.Severe, DeductionAmount = 5.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 30, Title = "إساءة معاملة العملاء أو المراجعين", Severity = ViolationSeverity.Serious, DeductionAmount = 3.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 31, Title = "عدم اتباع إجراءات السلامة المهنية", Severity = ViolationSeverity.Serious, DeductionAmount = 2.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 32, Title = "التجمهر أو عقد اجتماعات غير مصرح بها", Severity = ViolationSeverity.Moderate, DeductionAmount = 1.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 33, Title = "جمع تبرعات أو توزيع منشورات بدون إذن", Severity = ViolationSeverity.Moderate, DeductionAmount = 1.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 34, Title = "ممارسة أعمال تجارية خاصة داخل مقر العمل", Severity = ViolationSeverity.Serious, DeductionAmount = 3.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 35, Title = "جلب مواد خطرة أو محظورة لمقر العمل", Severity = ViolationSeverity.Severe, DeductionAmount = 5.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow },
                new() { RuleNumber = 36, Title = "التزوير في المحررات الرسمية أو الطبية", Severity = ViolationSeverity.Severe, DeductionAmount = 5.00m, TenantId = Guid.Empty, CreatedBy = "system", CreatedAt = DateTime.UtcNow }
            };

            context.Set<ViolationRule>().AddRange(rules);
            await context.SaveChangesAsync();
        }
    }
}
