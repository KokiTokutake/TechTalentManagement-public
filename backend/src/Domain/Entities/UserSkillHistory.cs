using TeckTalentManagement.Domain.Enums;

namespace TeckTalentManagement.Domain.Entities;

public class UserSkillHistory
{
    public Guid Id { get; private set; }
    public Guid UserSkillId { get; private set; }
    public int Level { get; private set; }
    public UserSkillStatus Status { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
    public Guid ChangedBy { get; private set; }

    public UserSkill? UserSkill { get; private set; }

    private UserSkillHistory() { }

    public static UserSkillHistory Create(Guid userSkillId, int level, UserSkillStatus status, Guid changedBy)
    {
        return new UserSkillHistory
        {
            Id = Guid.NewGuid(),
            UserSkillId = userSkillId,
            Level = level,
            Status = status,
            ChangedAt = DateTimeOffset.UtcNow,
            ChangedBy = changedBy
        };
    }
}
