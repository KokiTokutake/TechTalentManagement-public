namespace TeckTalentManagement.Application.NineBox.Responses;

public record NineBoxCalibrationResponse(
    Guid PeriodId,
    IReadOnlyList<NineBoxTeamCalibration> Teams,
    NineBoxCalibrationOverall Overall);

public record NineBoxTeamCalibration(
    Guid ManagerId,
    string ManagerName,
    int TalentCount,
    int AssessedCount,
    NineBoxDistribution Distribution,
    double AvgPerformanceScore,
    double AvgPotentialScore);

public record NineBoxCalibrationOverall(
    double AvgPerformanceScore,
    double AvgPotentialScore,
    NineBoxDistribution Distribution);

public record NineBoxDistribution(
    int HighHigh, int HighMid, int HighLow,
    int MidHigh, int MidMid, int MidLow,
    int LowHigh, int LowMid, int LowLow);
