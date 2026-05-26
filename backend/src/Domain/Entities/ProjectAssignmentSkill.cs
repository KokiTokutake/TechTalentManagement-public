namespace TechTalentManagement.Domain.Entities;

public class ProjectAssignmentSkill
{
    public Guid Id { get; private set; }
    public Guid AssignmentId { get; private set; }
    public Guid SkillItemId { get; private set; }

    public ProjectAssignment Assignment { get; private set; } = null!;
    public SkillItem SkillItem { get; private set; } = null!;

    private ProjectAssignmentSkill() { }

    public static ProjectAssignmentSkill Create(Guid assignmentId, Guid skillItemId)
    {
        return new ProjectAssignmentSkill
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignmentId,
            SkillItemId = skillItemId
        };
    }
}
