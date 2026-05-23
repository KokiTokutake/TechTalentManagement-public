using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Domain.Entities;

public class UserSkill
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid SkillItemId { get; private set; }
    public int Level { get; private set; }
    public string? Comment { get; private set; }
    public UserSkillStatus Status { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public string? RejectedReason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public User? User { get; private set; }
    public SkillItem? SkillItem { get; private set; }

    private UserSkill() { }

    public static UserSkill Create(Guid userId, Guid skillItemId, int level, string? comment)
    {
        var now = DateTimeOffset.UtcNow;
        return new UserSkill
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SkillItemId = skillItemId,
            Level = level,
            Comment = comment,
            Status = UserSkillStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(int level, string? comment)
    {
        Level = level;
        Comment = comment;
        Status = UserSkillStatus.Pending;
        ApprovedBy = null;
        ApprovedAt = null;
        RejectedReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Approve(Guid approverId)
    {
        Status = UserSkillStatus.Approved;
        ApprovedBy = approverId;
        ApprovedAt = DateTimeOffset.UtcNow;
        RejectedReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(string reason)
    {
        Status = UserSkillStatus.Rejected;
        RejectedReason = reason;
        ApprovedBy = null;
        ApprovedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
