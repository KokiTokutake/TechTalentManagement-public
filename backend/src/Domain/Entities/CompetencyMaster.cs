using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Domain.Entities;

public class CompetencyMaster
{
    public Guid Id { get; private set; }
    public CompetencyCategory Category { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private CompetencyMaster() { }

    public static CompetencyMaster Create(CompetencyCategory category, string name, string? description, int displayOrder)
    {
        var now = DateTimeOffset.UtcNow;
        return new CompetencyMaster
        {
            Id = Guid.NewGuid(),
            Category = category,
            Name = name,
            Description = description,
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(CompetencyCategory category, string name, string? description, int displayOrder, bool isActive)
    {
        Category = category;
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
