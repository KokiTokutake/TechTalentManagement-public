namespace TechTalentManagement.Application.UserSkills;

public record UserSkillDto(
    Guid Id,
    Guid UserId,
    Guid SkillItemId,
    string SkillItemName,
    Guid CategoryId,
    string CategoryName,
    int Level,
    string? Comment,
    string Status,
    Guid? ApprovedBy,
    DateTimeOffset? ApprovedAt,
    string? RejectedReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record AddUserSkillRequest(Guid SkillItemId, int Level, string? Comment);
public record UpdateUserSkillRequest(int Level, string? Comment);
public record RejectSkillRequest(string Reason);
