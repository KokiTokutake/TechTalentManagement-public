using NSubstitute;
using TeckTalentManagement.Application.Goals;
using TeckTalentManagement.Domain.Entities;
using TeckTalentManagement.Domain.Enums;

namespace TeckTalentManagement.Tests.Goals;

public class GoalServiceTests
{
    private readonly IGoalRepository _repo = Substitute.For<IGoalRepository>();
    private readonly GoalService _sut;

    public GoalServiceTests() => _sut = new GoalService(_repo);

    private static EvaluationPeriod OpenPeriod()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return EvaluationPeriod.Create(
            DateTime.UtcNow.Year, "テスト期間",
            today.AddDays(-10), today.AddDays(10),   // 目標登録期間：オープン
            today.AddDays(30), today.AddDays(60),
            today.AddDays(90));
    }

    private static EvaluationPeriod ClosedPeriod()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return EvaluationPeriod.Create(
            DateTime.UtcNow.Year - 1, "終了期間",
            today.AddDays(-60), today.AddDays(-30),  // 目標登録期間：クローズ
            today.AddDays(-20), today.AddDays(-10),
            today.AddDays(-5));
    }

    [Fact]
    public async Task CreateGoal_WhenPeriodNotFound_ReturnsNotFound()
    {
        _repo.FindPeriodAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
             .Returns((EvaluationPeriod?)null);

        var result = await _sut.CreateGoalAsync(Guid.NewGuid(), Guid.NewGuid(),
            new CreateGoalRequest(WillCanMust.Will, "目標タイトル", null, 1));

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task CreateGoal_WhenRegistrationClosed_ReturnsForbidden()
    {
        _repo.FindPeriodAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
             .Returns(ClosedPeriod());

        var result = await _sut.CreateGoalAsync(Guid.NewGuid(), Guid.NewGuid(),
            new CreateGoalRequest(WillCanMust.Will, "目標タイトル", null, 1));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task CreateGoal_WhenValid_ReturnsCreatedGoal()
    {
        var userId = Guid.NewGuid();
        var periodId = Guid.NewGuid();
        _repo.FindPeriodAsync(periodId, Arg.Any<CancellationToken>()).Returns(OpenPeriod());

        var result = await _sut.CreateGoalAsync(userId, periodId,
            new CreateGoalRequest(WillCanMust.Must, "テスト目標", "詳細", 1));

        Assert.True(result.IsSuccess);
        Assert.Equal("Must", result.Value!.Category);
        Assert.Equal("テスト目標", result.Value.Title);
        Assert.Equal(userId, result.Value.UserId);
    }

    [Fact]
    public async Task UpdateGoal_WhenGoalBelongsToOtherUser_ReturnsForbidden()
    {
        var goalOwnerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var period = OpenPeriod();
        var goal = Goal.Create(goalOwnerId, period.Id, WillCanMust.Will, "元のタイトル", null, 1);

        _repo.FindAsync(goal.Id, Arg.Any<CancellationToken>()).Returns(goal);
        _repo.FindPeriodAsync(period.Id, Arg.Any<CancellationToken>()).Returns(period);

        var result = await _sut.UpdateGoalAsync(otherUserId, goal.Id,
            new UpdateGoalRequest("変更後タイトル", null, 1));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task DeleteGoal_WhenValid_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var period = OpenPeriod();
        var goal = Goal.Create(userId, period.Id, WillCanMust.Can, "削除対象", null, 1);

        _repo.FindAsync(goal.Id, Arg.Any<CancellationToken>()).Returns(goal);
        _repo.FindPeriodAsync(period.Id, Arg.Any<CancellationToken>()).Returns(period);

        var result = await _sut.DeleteGoalAsync(userId, goal.Id);

        Assert.True(result.IsSuccess);
        await _repo.Received(1).DeleteAsync(goal, Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
