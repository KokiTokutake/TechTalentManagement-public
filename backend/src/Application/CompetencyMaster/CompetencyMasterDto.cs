using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Application.CompetencyMaster;

public record CompetencyMasterDto(
    Guid Id,
    string Category,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record CompetencyMastersResponse(IReadOnlyList<CompetencyMasterDto> Competencies);

public record CreateCompetencyMasterRequest(
    string Category,
    string Name,
    string? Description,
    int DisplayOrder);

public record UpdateCompetencyMasterRequest(
    string Category,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive);
