using NSubstitute;
using TechTalentManagement.Application.Notification;
using TechTalentManagement.Application.SelfEvaluations;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Tests.SelfEvaluations;

public class SelfEvaluationServiceTests
{
    private readonly ISelfEvaluationRepository _repo = Substitute.For<ISelfEvaluationRepository>();
    private readonly INotificationService _notificationService = Substitute.For<INotificationService>();
    private readonly SelfEvaluationService _sut;

    public SelfEvaluationServiceTests() => _sut = new SelfEvaluationService(_repo, _notificationService);

    private static EvaluationPeriod OpenPeriod()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return EvaluationPeriod.Create(
            DateTime.UtcNow.Year, "テスト期間",
            today.AddDays(-60), today.AddDays(-30),  // 目標登録期間：クローズ
            today.AddDays(-10), today.AddDays(10),   // 評価期間：オープン
            today.AddDays(30));
    }

    private static EvaluationPeriod ClosedPeriod()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return EvaluationPeriod.Create(
            DateTime.UtcNow.Year - 1, "終了期間",
            today.AddDays(-120), today.AddDays(-90),
            today.AddDays(-60), today.AddDays(-30),  // 評価期間：クローズ
            today.AddDays(-10));
    }

    [Fact]
    public async Task SaveDraft_WhenPeriodNotFound_ReturnsNotFound()
    {
        _repo.FindPeriodAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
             .Returns((EvaluationPeriod?)null);

        var result = await _sut.SaveDraftAsync(Guid.NewGuid(), Guid.NewGuid(),
            new SaveDraftRequest([]));

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task SaveDraft_WhenEvaluationPeriodClosed_ReturnsForbidden()
    {
        _repo.FindPeriodAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
             .Returns(ClosedPeriod());

        var result = await _sut.SaveDraftAsync(Guid.NewGuid(), Guid.NewGuid(),
            new SaveDraftRequest([]));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task SaveDraft_WhenAlreadySubmitted_ReturnsConflict()
    {
        var userId = Guid.NewGuid();
        var period = OpenPeriod();
        var evaluation = SelfEvaluation.Create(userId, period.Id);
        evaluation.Submit();

        _repo.FindPeriodAsync(period.Id, Arg.Any<CancellationToken>()).Returns(period);
        _repo.FindAsync(userId, period.Id, Arg.Any<CancellationToken>()).Returns(evaluation);
        _repo.GetGoalsByUserAndPeriodAsync(userId, period.Id, Arg.Any<CancellationToken>())
             .Returns(new List<Goal>());

        var result = await _sut.SaveDraftAsync(userId, period.Id, new SaveDraftRequest([]));

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task SaveDraft_WhenValid_CreatesDraftAndReturnsDto()
    {
        var userId = Guid.NewGuid();
        var period = OpenPeriod();
        var goal = Goal.Create(userId, period.Id, Domain.Enums.WillCanMust.Must, "目標", null, 1);

        _repo.FindPeriodAsync(period.Id, Arg.Any<CancellationToken>()).Returns(period);
        _repo.FindAsync(userId, period.Id, Arg.Any<CancellationToken>()).Returns((SelfEvaluation?)null);
        _repo.GetGoalsByUserAndPeriodAsync(userId, period.Id, Arg.Any<CancellationToken>())
             .Returns(new List<Goal> { goal });

        var req = new SaveDraftRequest([new SaveDraftItemRequest(goal.Id, 4, "良い達成度")]);
        var result = await _sut.SaveDraftAsync(userId, period.Id, req);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.IsSubmitted);
        Assert.Single(result.Value.Items);
        Assert.Equal(4, result.Value.Items[0].AchievementScore);
        await _repo.Received(1).AddAsync(Arg.Any<SelfEvaluation>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Submit_WhenAlreadySubmitted_ReturnsConflict()
    {
        var userId = Guid.NewGuid();
        var period = OpenPeriod();
        var evaluation = SelfEvaluation.Create(userId, period.Id);
        evaluation.Submit();

        _repo.FindPeriodAsync(period.Id, Arg.Any<CancellationToken>()).Returns(period);
        _repo.FindAsync(userId, period.Id, Arg.Any<CancellationToken>()).Returns(evaluation);

        var result = await _sut.SubmitAsync(userId, period.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task Submit_WhenValid_SetsIsSubmittedAndReturnsDto()
    {
        var userId = Guid.NewGuid();
        var period = OpenPeriod();
        var evaluation = SelfEvaluation.Create(userId, period.Id);

        _repo.FindPeriodAsync(period.Id, Arg.Any<CancellationToken>()).Returns(period);
        _repo.FindAsync(userId, period.Id, Arg.Any<CancellationToken>()).Returns(evaluation);

        var result = await _sut.SubmitAsync(userId, period.Id);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsSubmitted);
        Assert.NotNull(result.Value.SubmittedAt);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
