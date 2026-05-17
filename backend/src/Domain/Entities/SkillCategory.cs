namespace TeckTalentManagement.Domain.Entities;

public class SkillCategory
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private readonly List<SkillItem> _items = [];
    public IReadOnlyList<SkillItem> Items => _items.AsReadOnly();

    private SkillCategory() { }

    public static SkillCategory Create(string name, int displayOrder)
    {
        var now = DateTimeOffset.UtcNow;
        return new SkillCategory
        {
            Id = Guid.NewGuid(),
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
