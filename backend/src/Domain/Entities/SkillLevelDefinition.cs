namespace TechTalentManagement.Domain.Entities;

public class SkillLevelDefinition
{
    public Guid SkillItemId { get; private set; }
    public int Level { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? Examples { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public SkillItem? SkillItem { get; private set; }

    private SkillLevelDefinition() { }

    public static SkillLevelDefinition Create(Guid skillItemId, int level, string description, string? examples)
    {
        var now = DateTimeOffset.UtcNow;
        return new SkillLevelDefinition
        {
            SkillItemId = skillItemId,
            Level = level,
            Description = description,
            Examples = examples,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string description, string? examples)
    {
        Description = description;
        Examples = examples;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
