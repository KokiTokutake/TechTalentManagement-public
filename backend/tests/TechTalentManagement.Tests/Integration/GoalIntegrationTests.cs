using TechTalentManagement.Application.Goals;
using TechTalentManagement.Domain.Enums;
using TechTalentManagement.Infrastructure.Persistence.Repositories;

namespace TechTalentManagement.Tests.Integration;

// 受入基準: REQ-01 タレントが目標を登録・編集・削除できる
public class GoalIntegrationTests : IntegrationTestBase
{
    private readonly GoalService _sut;

    public GoalIntegrationTests()
    {
        var repo = new GoalRepository(Db);
        _sut = new GoalService(repo);
    }

    [Fact]
    public async Task AC_Goal_01_目標登録期間内にWillCanMust各軸で目標を作成できる()
    {
        var period = CreateOpenGoalPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();
        var userId = Guid.NewGuid();

        var willResult = await _sut.CreateGoalAsync(userId, period.Id,
            new CreateGoalRequest(WillCanMust.Will, "将来やりたいこと", null, 1));
        var canResult = await _sut.CreateGoalAsync(userId, period.Id,
            new CreateGoalRequest(WillCanMust.Can, "今できること", null, 1));
        var mustResult = await _sut.CreateGoalAsync(userId, period.Id,
            new CreateGoalRequest(WillCanMust.Must, "やるべきこと", null, 1));

        Assert.True(willResult.IsSuccess);
        Assert.True(canResult.IsSuccess);
        Assert.True(mustResult.IsSuccess);
        Assert.Equal("Will", willResult.Value!.Category);
        Assert.Equal("Can", canResult.Value!.Category);
        Assert.Equal("Must", mustResult.Value!.Category);
    }

    [Fact]
    public async Task AC_Goal_02_目標登録期間外は403が返る()
    {
        var period = CreateClosedPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();

        var result = await _sut.CreateGoalAsync(Guid.NewGuid(), period.Id,
            new CreateGoalRequest(WillCanMust.Will, "テスト", null, 1));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task AC_Goal_03_登録した目標を一覧取得できる()
    {
        var period = CreateOpenGoalPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();
        var userId = Guid.NewGuid();

        await _sut.CreateGoalAsync(userId, period.Id, new CreateGoalRequest(WillCanMust.Will, "目標1", null, 1));
        await _sut.CreateGoalAsync(userId, period.Id, new CreateGoalRequest(WillCanMust.Must, "目標2", null, 2));

        var listResult = await _sut.GetGoalsAsync(userId, period.Id);

        Assert.True(listResult.IsSuccess);
        Assert.Equal(2, listResult.Value!.Count);
    }

    [Fact]
    public async Task AC_Goal_04_他ユーザーの目標は編集できない()
    {
        var period = CreateOpenGoalPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();
        var ownerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();

        var created = await _sut.CreateGoalAsync(ownerId, period.Id,
            new CreateGoalRequest(WillCanMust.Will, "元のタイトル", null, 1));

        var updateResult = await _sut.UpdateGoalAsync(otherId, created.Value!.Id,
            new UpdateGoalRequest("変更後", null, 1));

        Assert.False(updateResult.IsSuccess);
        Assert.Equal(403, updateResult.StatusCode);
    }

    [Fact]
    public async Task AC_Goal_05_目標を削除するとDBからも除去される()
    {
        var period = CreateOpenGoalPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();
        var userId = Guid.NewGuid();

        var created = await _sut.CreateGoalAsync(userId, period.Id,
            new CreateGoalRequest(WillCanMust.Can, "削除対象", null, 1));
        var goalId = created.Value!.Id;

        await _sut.DeleteGoalAsync(userId, goalId);
        var listResult = await _sut.GetGoalsAsync(userId, period.Id);

        Assert.True(listResult.IsSuccess);
        Assert.Empty(listResult.Value!);
    }
}
