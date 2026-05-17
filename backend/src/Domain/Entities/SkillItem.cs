namespace TeckTalentManagement.Domain.Entities;

public class SkillItem
{
    public Guid Id { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public SkillCategory? Category { get; private set; }

    private SkillItem() { }

    public static SkillItem Create(Guid categoryId, string name, int displayOrder)
    {
        var now = DateTimeOffset.UtcNow;
        return new SkillItem
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Name = name,
            DisplayOrder = displayOrder,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(string name, int displayOrder)
    {
        Name = name;
        DisplayOrder = displayOrder;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
