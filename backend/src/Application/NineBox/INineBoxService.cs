using TechTalentManagement.Application.Common;
using TechTalentManagement.Application.NineBox.Requests;
using TechTalentManagement.Application.NineBox.Responses;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Application.NineBox;

public interface INineBoxService
{
    Task<Result<NineBoxAssessmentResponse>> CreateAssessmentAsync(Guid assessorId, CreateNineBoxAssessmentRequest req, CancellationToken ct = default);
    Task<Result<NineBoxAssessmentResponse>> UpdateAssessmentAsync(Guid assessorId, Guid assessmentId, UpdateNineBoxAssessmentRequest req, CancellationToken ct = default);
    Task<Result<IReadOnlyList<NineBoxCellResponse>>> GetCellDefinitionsAsync(CancellationToken ct = default);
    Task<Result<NineBoxMapResponse>> GetMapAsync(Guid periodId, Guid? departmentId, string? role, string? searchName, Guid? comparePeriodId, CancellationToken ct = default);
    Task<Result<NineBoxHistoryResponse>> GetHistoryAsync(Guid talentId, CancellationToken ct = default);
    Task<Result<NineBoxCalibrationResponse>> GetCalibrationAsync(Guid periodId, CancellationToken ct = default);
    Task<Result<int>> FlagHighPotentialCandidatesAsync(Guid periodId, Guid requesterId, CancellationToken ct = default);
}

public interface INineBoxRepository
{
    Task<EvaluationPeriod?> FindPeriodAsync(Guid periodId, CancellationToken ct);
    Task<User?> FindUserAsync(Guid userId, CancellationToken ct);
    Task<NineBoxAssessment?> FindAssessmentAsync(Guid assessmentId, CancellationToken ct);
    Task<NineBoxAssessment?> FindAssessmentByTalentAndPeriodAsync(Guid talentId, Guid periodId, CancellationToken ct);
    Task<IReadOnlyList<NineBoxCellDefinition>> GetCellDefinitionsAsync(CancellationToken ct);
    Task<IReadOnlyList<NineBoxAssessment>> GetAssessmentsWithTalentsForPeriodAsync(Guid periodId, CancellationToken ct);
    Task<IReadOnlyList<NineBoxAssessment>> GetAssessmentsWithUsersForPeriodAsync(Guid periodId, CancellationToken ct);
    Task<IReadOnlyList<NineBoxAssessment>> GetAssessmentsForTalentAsync(Guid talentId, CancellationToken ct);
    Task<int> GetTalentCountAsync(CancellationToken ct);
    Task<IReadOnlyList<User>> GetManagersWithTalentsAsync(CancellationToken ct);
    Task<IReadOnlyList<Domain.Entities.KeyPerson>> GetActiveKeyPersonsAsync(CancellationToken ct);
    Task AddKeyPersonAsync(Domain.Entities.KeyPerson keyPerson, CancellationToken ct);
    Task AddAsync(NineBoxAssessment assessment, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public class NineBoxService(INineBoxRepository repo) : INineBoxService
{
    public async Task<Result<NineBoxAssessmentResponse>> CreateAssessmentAsync(
        Guid assessorId,
        CreateNineBoxAssessmentRequest req,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<PerformanceLevel>(req.PerformanceLevel, out var perf))
            return Result<NineBoxAssessmentResponse>.BadRequest($"PerformanceLevel の値が不正です: {req.PerformanceLevel}");

        if (!Enum.TryParse<PotentialLevel>(req.PotentialLevel, out var pot))
            return Result<NineBoxAssessmentResponse>.BadRequest($"PotentialLevel の値が不正です: {req.PotentialLevel}");

        var period = await repo.FindPeriodAsync(req.PeriodId, ct);
        if (period is null)
            return Result<NineBoxAssessmentResponse>.NotFound("考課期間が見つかりません。");

        if (!period.IsEvaluationOpen(DateOnly.FromDateTime(DateTime.UtcNow)))
            return Result<NineBoxAssessmentResponse>.Forbidden("評価入力可能な期間ではありません。");

        var talent = await repo.FindUserAsync(req.TalentId, ct);
        if (talent is null)
            return Result<NineBoxAssessmentResponse>.NotFound("対象タレントが見つかりません。");

        var existing = await repo.FindAssessmentByTalentAndPeriodAsync(req.TalentId, req.PeriodId, ct);
        if (existing is not null)
            return Result<NineBoxAssessmentResponse>.Conflict("この期間に既に評価が存在します。更新する場合は PUT を使用してください。");

        var assessment = NineBoxAssessment.Create(
            req.TalentId, req.PeriodId, assessorId,
            perf, pot,
            req.PerformanceRationale, req.PotentialRationale);

        await repo.AddAsync(assessment, ct);
        await repo.SaveChangesAsync(ct);

        return Result<NineBoxAssessmentResponse>.Ok(ToResponse(assessment, talent.DisplayName));
    }

    public async Task<Result<NineBoxAssessmentResponse>> UpdateAssessmentAsync(
        Guid assessorId,
        Guid assessmentId,
        UpdateNineBoxAssessmentRequest req,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<PerformanceLevel>(req.PerformanceLevel, out var perf))
            return Result<NineBoxAssessmentResponse>.BadRequest($"PerformanceLevel の値が不正です: {req.PerformanceLevel}");

        if (!Enum.TryParse<PotentialLevel>(req.PotentialLevel, out var pot))
            return Result<NineBoxAssessmentResponse>.BadRequest($"PotentialLevel の値が不正です: {req.PotentialLevel}");

        var assessment = await repo.FindAssessmentAsync(assessmentId, ct);
        if (assessment is null)
            return Result<NineBoxAssessmentResponse>.NotFound("評価が見つかりません。");

        if (assessment.AssessorId != assessorId)
            return Result<NineBoxAssessmentResponse>.Forbidden("他のアセッサーの評価は更新できません。");

        var talent = await repo.FindUserAsync(assessment.TalentId, ct);

        assessment.Update(perf, pot, req.PerformanceRationale, req.PotentialRationale);
        await repo.SaveChangesAsync(ct);

        return Result<NineBoxAssessmentResponse>.Ok(ToResponse(assessment, talent?.DisplayName ?? string.Empty));
    }

    public async Task<Result<IReadOnlyList<NineBoxCellResponse>>> GetCellDefinitionsAsync(CancellationToken ct = default)
    {
        var cells = await repo.GetCellDefinitionsAsync(ct);
        var result = cells
            .Select(c => new NineBoxCellResponse(
                c.PerformanceLevel.ToString(),
                c.PotentialLevel.ToString(),
                c.Label,
                c.Description,
                c.Color))
            .ToList();
        return Result<IReadOnlyList<NineBoxCellResponse>>.Ok(result);
    }

    public async Task<Result<NineBoxMapResponse>> GetMapAsync(
        Guid periodId,
        Guid? departmentId,
        string? role,
        string? searchName,
        Guid? comparePeriodId,
        CancellationToken ct = default)
    {
        var period = await repo.FindPeriodAsync(periodId, ct);
        if (period is null)
            return Result<NineBoxMapResponse>.NotFound("考課期間が見つかりません。");

        var cells = await repo.GetCellDefinitionsAsync(ct);
        var cellMap = cells.ToDictionary(c => (c.PerformanceLevel, c.PotentialLevel));

        var assessments = await repo.GetAssessmentsWithTalentsForPeriodAsync(periodId, ct);

        // フィルタ
        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, out var roleEnum))
            assessments = assessments.Where(a => a.Talent.Role == roleEnum).ToList();

        if (!string.IsNullOrWhiteSpace(searchName))
            assessments = assessments.Where(a => a.Talent.DisplayName.Contains(searchName, StringComparison.OrdinalIgnoreCase)).ToList();

        // 前期比較データ
        Dictionary<Guid, NineBoxAssessment> prevMap = [];
        if (comparePeriodId.HasValue)
        {
            var prevAssessments = await repo.GetAssessmentsWithTalentsForPeriodAsync(comparePeriodId.Value, ct);
            prevMap = prevAssessments.ToDictionary(a => a.TalentId);
        }

        var totalTalents = await repo.GetTalentCountAsync(ct);
        var highPotentialCount = 0;

        // 9セル全て生成（タレントがいないセルも空で含める）
        var allCells = new List<NineBoxMapCell>();
        foreach (var perf in new[] { PerformanceLevel.High, PerformanceLevel.Mid, PerformanceLevel.Low })
        {
            foreach (var pot in new[] { PerformanceLevel.High, PerformanceLevel.Mid, PerformanceLevel.Low })
            {
                var potLevel = (PotentialLevel)(int)pot;
                var cellAssessments = assessments.Where(a => a.PerformanceLevel == perf && a.PotentialLevel == potLevel).ToList();
                cellMap.TryGetValue((perf, potLevel), out var cellDef);

                var talentItems = cellAssessments.Select(a =>
                {
                    var isHp = IsHighPotentialCandidate(a.PerformanceLevel, a.PotentialLevel);
                    if (isHp) highPotentialCount++;

                    NineBoxPreviousCell? prevCell = null;
                    string? movement = null;
                    if (prevMap.TryGetValue(a.TalentId, out var prev))
                    {
                        prevCell = new NineBoxPreviousCell(prev.PerformanceLevel.ToString(), prev.PotentialLevel.ToString());
                        var perfDiff = (int)a.PerformanceLevel - (int)prev.PerformanceLevel;
                        var potDiff = (int)a.PotentialLevel - (int)prev.PotentialLevel;
                        var totalDiff = perfDiff + potDiff;
                        movement = totalDiff > 0 ? "up" : totalDiff < 0 ? "down" : "same";
                    }

                    return new NineBoxTalentItem(
                        a.Id, a.TalentId, a.Talent.DisplayName,
                        BuildInitials(a.Talent.DisplayName),
                        a.Talent.Role.ToString(),
                        a.PerformanceRationale, a.PotentialRationale,
                        isHp, prevCell, movement);
                }).ToList();

                allCells.Add(new NineBoxMapCell(
                    perf.ToString(), potLevel.ToString(),
                    cellDef?.Label ?? string.Empty,
                    cellDef?.Color ?? "#6b7280",
                    cellAssessments.Count,
                    talentItems));
            }
        }

        var summary = new NineBoxMapSummary(assessments.Count, totalTalents, highPotentialCount);
        return Result<NineBoxMapResponse>.Ok(new NineBoxMapResponse(periodId, period.Name, allCells, summary));
    }

    public async Task<Result<NineBoxHistoryResponse>> GetHistoryAsync(Guid talentId, CancellationToken ct = default)
    {
        var talent = await repo.FindUserAsync(talentId, ct);
        if (talent is null)
            return Result<NineBoxHistoryResponse>.NotFound("タレントが見つかりません。");

        var assessments = await repo.GetAssessmentsForTalentAsync(talentId, ct);
        var cells = await repo.GetCellDefinitionsAsync(ct);
        var cellMap = cells.ToDictionary(c => (c.PerformanceLevel, c.PotentialLevel));

        var history = assessments
            .OrderBy(a => a.EvaluationPeriod.Year)
            .Select(a =>
            {
                cellMap.TryGetValue((a.PerformanceLevel, a.PotentialLevel), out var cellDef);
                return new NineBoxHistoryItem(
                    a.EvaluationPeriodId,
                    a.EvaluationPeriod.Name,
                    a.PerformanceLevel.ToString(),
                    a.PotentialLevel.ToString(),
                    cellDef?.Label ?? string.Empty,
                    a.AssessedAt);
            })
            .ToList();

        return Result<NineBoxHistoryResponse>.Ok(new NineBoxHistoryResponse(talentId, talent.DisplayName, history));
    }

    public async Task<Result<NineBoxCalibrationResponse>> GetCalibrationAsync(Guid periodId, CancellationToken ct = default)
    {
        var period = await repo.FindPeriodAsync(periodId, ct);
        if (period is null)
            return Result<NineBoxCalibrationResponse>.NotFound("考課期間が見つかりません。");

        var assessments = await repo.GetAssessmentsWithUsersForPeriodAsync(periodId, ct);
        var managers = await repo.GetManagersWithTalentsAsync(ct);

        var teamCalibrations = new List<NineBoxTeamCalibration>();
        foreach (var manager in managers)
        {
            var managedTalentIds = assessments
                .Where(a => a.Talent.ManagerId == manager.Id)
                .Select(a => a.TalentId)
                .ToHashSet();

            var teamAssessments = assessments.Where(a => managedTalentIds.Contains(a.TalentId)).ToList();
            var talentCount = managers.Count(m => m.Id == manager.Id); // counts self - use talent navigator instead

            // マネージャー配下のタレント数は Assessor 別で集計
            var byAssessor = assessments.Where(a => a.AssessorId == manager.Id).ToList();
            if (!byAssessor.Any()) continue;

            var dist = BuildDistribution(byAssessor);
            var avgPerf = byAssessor.Average(a => (double)(int)a.PerformanceLevel);
            var avgPot = byAssessor.Average(a => (double)(int)a.PotentialLevel);

            teamCalibrations.Add(new NineBoxTeamCalibration(
                manager.Id, manager.DisplayName,
                byAssessor.Count, byAssessor.Count,
                dist, avgPerf, avgPot));
        }

        var overallDist = BuildDistribution(assessments);
        var overallAvgPerf = assessments.Count > 0 ? assessments.Average(a => (double)(int)a.PerformanceLevel) : 0;
        var overallAvgPot = assessments.Count > 0 ? assessments.Average(a => (double)(int)a.PotentialLevel) : 0;
        var overall = new NineBoxCalibrationOverall(overallAvgPerf, overallAvgPot, overallDist);

        return Result<NineBoxCalibrationResponse>.Ok(new NineBoxCalibrationResponse(periodId, teamCalibrations, overall));
    }

    public async Task<Result<int>> FlagHighPotentialCandidatesAsync(Guid periodId, Guid requesterId, CancellationToken ct = default)
    {
        var period = await repo.FindPeriodAsync(periodId, ct);
        if (period is null)
            return Result<int>.NotFound("考課期間が見つかりません。");

        var assessments = await repo.GetAssessmentsWithTalentsForPeriodAsync(periodId, ct);
        var highPotentialTalentIds = assessments
            .Where(a => IsHighPotentialCandidate(a.PerformanceLevel, a.PotentialLevel))
            .Select(a => a.TalentId)
            .ToHashSet();

        var activeKeyPersons = await repo.GetActiveKeyPersonsAsync(ct);
        var currentKeyPersonTalentIds = activeKeyPersons.Select(kp => kp.TalentId).ToHashSet();

        int changeCount = 0;

        // 新規に候補になったタレントを追加
        foreach (var talentId in highPotentialTalentIds.Except(currentKeyPersonTalentIds))
        {
            var keyPerson = Domain.Entities.KeyPerson.Create(talentId, requesterId, "9ボックス評価によるHighPotential候補の自動推薦");
            await repo.AddKeyPersonAsync(keyPerson, ct);
            changeCount++;
        }

        // 候補から外れたタレントのフラグを解除
        foreach (var kp in activeKeyPersons.Where(kp => !highPotentialTalentIds.Contains(kp.TalentId)))
        {
            kp.Deactivate();
            changeCount++;
        }

        await repo.SaveChangesAsync(ct);
        return Result<int>.Ok(changeCount);
    }

    private static NineBoxDistribution BuildDistribution(IReadOnlyList<NineBoxAssessment> assessments)
    {
        int Count(PerformanceLevel p, PotentialLevel pot) =>
            assessments.Count(a => a.PerformanceLevel == p && a.PotentialLevel == pot);

        return new NineBoxDistribution(
            Count(PerformanceLevel.High, PotentialLevel.High),
            Count(PerformanceLevel.High, PotentialLevel.Mid),
            Count(PerformanceLevel.High, PotentialLevel.Low),
            Count(PerformanceLevel.Mid, PotentialLevel.High),
            Count(PerformanceLevel.Mid, PotentialLevel.Mid),
            Count(PerformanceLevel.Mid, PotentialLevel.Low),
            Count(PerformanceLevel.Low, PotentialLevel.High),
            Count(PerformanceLevel.Low, PotentialLevel.Mid),
            Count(PerformanceLevel.Low, PotentialLevel.Low));
    }

    private static string BuildInitials(string displayName)
    {
        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? string.Concat(parts[0].AsSpan(0, 1), parts[1].AsSpan(0, 1))
            : displayName.Length >= 2 ? displayName[..2] : displayName;
    }

    private static NineBoxAssessmentResponse ToResponse(NineBoxAssessment a, string talentName) =>
        new(a.Id, a.TalentId, talentName, a.EvaluationPeriodId,
            a.PerformanceLevel.ToString(), a.PotentialLevel.ToString(),
            a.PerformanceRationale, a.PotentialRationale,
            IsHighPotentialCandidate(a.PerformanceLevel, a.PotentialLevel),
            a.AssessedAt);

    private static bool IsHighPotentialCandidate(PerformanceLevel perf, PotentialLevel pot) =>
        (perf == PerformanceLevel.High && pot >= PotentialLevel.Mid) ||
        (perf == PerformanceLevel.Mid && pot == PotentialLevel.High);
}
