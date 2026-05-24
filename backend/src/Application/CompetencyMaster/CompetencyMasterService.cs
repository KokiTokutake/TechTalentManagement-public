using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Application.CompetencyMaster;

public interface ICompetencyMasterService
{
    Task<Result<CompetencyMastersResponse>> GetActiveCompetenciesAsync(CancellationToken ct = default);
    Task<Result<CompetencyMasterDto>> CreateCompetencyAsync(Guid requesterId, CreateCompetencyMasterRequest req, CancellationToken ct = default);
    Task<Result<CompetencyMasterDto>> UpdateCompetencyAsync(Guid requesterId, Guid competencyId, UpdateCompetencyMasterRequest req, CancellationToken ct = default);
}

public interface ICompetencyMasterRepository
{
    Task<bool> IsAdminAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<Domain.Entities.CompetencyMaster>> GetActiveAsync(CancellationToken ct);
    Task<Domain.Entities.CompetencyMaster?> FindAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsByCategoryAndNameAsync(CompetencyCategory category, string name, Guid? excludeId, CancellationToken ct);
    Task AddAsync(Domain.Entities.CompetencyMaster competency, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public class CompetencyMasterService(ICompetencyMasterRepository repo) : ICompetencyMasterService
{
    public async Task<Result<CompetencyMastersResponse>> GetActiveCompetenciesAsync(CancellationToken ct = default)
    {
        var competencies = await repo.GetActiveAsync(ct);
        var dtos = competencies.Select(ToDto).ToList();
        return Result<CompetencyMastersResponse>.Ok(new CompetencyMastersResponse(dtos));
    }

    public async Task<Result<CompetencyMasterDto>> CreateCompetencyAsync(Guid requesterId, CreateCompetencyMasterRequest req, CancellationToken ct = default)
    {
        if (!await repo.IsAdminAsync(requesterId, ct))
            return Result<CompetencyMasterDto>.Forbidden("管理者のみ操作できます。");

        if (!Enum.TryParse<CompetencyCategory>(req.Category, out var category))
            return Result<CompetencyMasterDto>.BadRequest($"無効なカテゴリです: {req.Category}");

        if (await repo.ExistsByCategoryAndNameAsync(category, req.Name, null, ct))
            return Result<CompetencyMasterDto>.Conflict("同じカテゴリに同名のコンピテンシーが既に存在します。");

        var competency = Domain.Entities.CompetencyMaster.Create(category, req.Name, req.Description, req.DisplayOrder);
        await repo.AddAsync(competency, ct);
        await repo.SaveChangesAsync(ct);

        return Result<CompetencyMasterDto>.Created(ToDto(competency));
    }

    public async Task<Result<CompetencyMasterDto>> UpdateCompetencyAsync(Guid requesterId, Guid competencyId, UpdateCompetencyMasterRequest req, CancellationToken ct = default)
    {
        if (!await repo.IsAdminAsync(requesterId, ct))
            return Result<CompetencyMasterDto>.Forbidden("管理者のみ操作できます。");

        var competency = await repo.FindAsync(competencyId, ct);
        if (competency is null)
            return Result<CompetencyMasterDto>.NotFound("コンピテンシーが見つかりません。");

        if (!Enum.TryParse<CompetencyCategory>(req.Category, out var category))
            return Result<CompetencyMasterDto>.BadRequest($"無効なカテゴリです: {req.Category}");

        if (await repo.ExistsByCategoryAndNameAsync(category, req.Name, competencyId, ct))
            return Result<CompetencyMasterDto>.Conflict("同じカテゴリに同名のコンピテンシーが既に存在します。");

        competency.Update(category, req.Name, req.Description, req.DisplayOrder, req.IsActive);
        await repo.SaveChangesAsync(ct);

        return Result<CompetencyMasterDto>.Ok(ToDto(competency));
    }

    private static CompetencyMasterDto ToDto(Domain.Entities.CompetencyMaster c) =>
        new(c.Id, c.Category.ToString(), c.Name, c.Description, c.DisplayOrder, c.IsActive, c.CreatedAt, c.UpdatedAt);
}
