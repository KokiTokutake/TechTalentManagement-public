using TeckTalentManagement.Domain.Enums;

namespace TeckTalentManagement.Domain.Entities;

public class NineBoxCellDefinition
{
    public Guid Id { get; private set; }
    public PerformanceLevel PerformanceLevel { get; private set; }
    public PotentialLevel PotentialLevel { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;

    private NineBoxCellDefinition() { }

    public static NineBoxCellDefinition Create(
        PerformanceLevel performanceLevel,
        PotentialLevel potentialLevel,
        string label,
        string description,
        string color)
    {
        return new NineBoxCellDefinition
        {
            Id = Guid.NewGuid(),
            PerformanceLevel = performanceLevel,
            PotentialLevel = potentialLevel,
            Label = label,
            Description = description,
            Color = color
        };
    }

    public void Update(string label, string description, string color)
    {
        Label = label;
        Description = description;
        Color = color;
    }
}
