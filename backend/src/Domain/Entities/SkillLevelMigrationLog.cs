namespace TechTalentManagement.Domain.Entities;

public class SkillLevelMigrationLog
{
    public Guid Id { get; private set; }
    public Guid ExecutedBy { get; private set; }
    public DateTimeOffset ExecutedAt { get; private set; }
    public int AffectedCount { get; private set; }
    public string ConversionRuleJson { get; private set; } = string.Empty;

    public User? Executor { get; private set; }

    private SkillLevelMigrationLog() { }

    public static SkillLevelMigrationLog Create(Guid executedBy, int affectedCount, string conversionRuleJson)
    {
        return new SkillLevelMigrationLog
        {
            Id = Guid.NewGuid(),
            ExecutedBy = executedBy,
            ExecutedAt = DateTimeOffset.UtcNow,
            AffectedCount = affectedCount,
            ConversionRuleJson = conversionRuleJson
        };
    }
}
