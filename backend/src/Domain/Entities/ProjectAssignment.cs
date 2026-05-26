namespace TechTalentManagement.Domain.Entities;

public class ProjectAssignment
{
    public Guid Id { get; private set; }
    public Guid TalentId { get; private set; }
    public string ProjectName { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public string? Description { get; private set; }
    public string? ManagerComment { get; private set; }
    public Guid? ManagerCommentedById { get; private set; }
    public DateTimeOffset? ManagerCommentedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public User Talent { get; private set; } = null!;
    public User? ManagerCommentedBy { get; private set; }
    public ICollection<ProjectAssignmentSkill> Skills { get; private set; } = [];

    private ProjectAssignment() { }

    public static ProjectAssignment Create(
        Guid talentId,
        string projectName,
        string role,
        DateOnly startDate,
        DateOnly? endDate,
        string? description)
    {
        var now = DateTimeOffset.UtcNow;
        return new ProjectAssignment
        {
            Id = Guid.NewGuid(),
            TalentId = talentId,
            ProjectName = projectName,
            Role = role,
            StartDate = startDate,
            EndDate = endDate,
            Description = description,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(
        string projectName,
        string role,
        DateOnly startDate,
        DateOnly? endDate,
        string? description)
    {
        ProjectName = projectName;
        Role = role;
        StartDate = startDate;
        EndDate = endDate;
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetManagerComment(string comment, Guid commentedById)
    {
        ManagerComment = comment;
        ManagerCommentedById = commentedById;
        ManagerCommentedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
