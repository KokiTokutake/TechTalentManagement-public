using NSubstitute;
using TechTalentManagement.Application.Notification;
using TechTalentManagement.Application.SelfEvaluations;
using TechTalentManagement.Domain.Enums;
using TechTalentManagement.Infrastructure.Persistence.Repositories;

namespace TechTalentManagement.Tests.Integration;

// 受入基準: REQ-02 タレントが自己評価を入力・提出できる
public class SelfEvaluationIntegrationTests : IntegrationTestBase
{
    private readonly SelfEvaluationService _sut;

    public SelfEvaluationIntegrationTests()
    {
        _sut = new SelfEvaluationService(new SelfEvaluationRepository(Db), Substitute.For<INotificationService>());
    }

    // 評価期間中は目標登録期間が終了しているため、DBに直接シードするヘルパー
    private async Task<TechTalentManagement.Domain.Entities.Goal> SeedGoal(Guid userId, Guid periodId)
    {
        var goal = TechTalentManagement.Domain.Entities.Goal.Create(
            userId, periodId, WillCanMust.Must, "テスト目標", null, 1);
        await Db.Goals.AddAsync(goal);
        await Db.SaveChangesAsync();
        return goal;
    }

    [Fact]
    public async Task AC_SelfEval_01_目標ごとに達成度とコメントを下書き保存できる()
    {
        var period = CreateOpenEvalPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();
        var userId = Guid.NewGuid();
        var goal = await SeedGoal(userId, period.Id);

        var draftResult = await _sut.SaveDraftAsync(userId, period.Id, new SaveDraftRequest([
            new SaveDraftItemRequest(goal.Id, 4, "よく達成できました")
        ]));

        Assert.True(draftResult.IsSuccess);
        Assert.False(draftResult.Value!.IsSubmitted);
        Assert.Single(draftResult.Value.Items);
        Assert.Equal(4, draftResult.Value.Items[0].AchievementScore);
        Assert.Equal("よく達成できました", draftResult.Value.Items[0].Comment);
    }

    [Fact]
    public async Task AC_SelfEval_02_下書きを複数回上書きできる()
    {
        var period = CreateOpenEvalPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();
        var userId = Guid.NewGuid();
        var goal = await SeedGoal(userId, period.Id);

        await _sut.SaveDraftAsync(userId, period.Id, new SaveDraftRequest([
            new SaveDraftItemRequest(goal.Id, 3, "最初のコメント")
        ]));

        var secondDraft = await _sut.SaveDraftAsync(userId, period.Id, new SaveDraftRequest([
            new SaveDraftItemRequest(goal.Id, 5, "修正後コメント")
        ]));

        Assert.True(secondDraft.IsSuccess);
        Assert.Equal(5, secondDraft.Value!.Items[0].AchievementScore);
        Assert.Equal("修正後コメント", secondDraft.Value.Items[0].Comment);
    }

    [Fact]
    public async Task AC_SelfEval_03_提出すると自己評価が確定しIsSubmittedがtrueになる()
    {
        var period = CreateOpenEvalPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();
        var userId = Guid.NewGuid();
        var goal = await SeedGoal(userId, period.Id);

        await _sut.SaveDraftAsync(userId, period.Id, new SaveDraftRequest([
            new SaveDraftItemRequest(goal.Id, 4, null)
        ]));

        var submitResult = await _sut.SubmitAsync(userId, period.Id);

        Assert.True(submitResult.IsSuccess);
        Assert.True(submitResult.Value!.IsSubmitted);
        Assert.NotNull(submitResult.Value.SubmittedAt);
    }

    [Fact]
    public async Task AC_SelfEval_04_提出後に下書き保存しようとすると409が返る()
    {
        var period = CreateOpenEvalPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();
        var userId = Guid.NewGuid();
        var goal = await SeedGoal(userId, period.Id);

        await _sut.SaveDraftAsync(userId, period.Id, new SaveDraftRequest([
            new SaveDraftItemRequest(goal.Id, 3, null)
        ]));
        await _sut.SubmitAsync(userId, period.Id);

        var editAfterSubmit = await _sut.SaveDraftAsync(userId, period.Id, new SaveDraftRequest([
            new SaveDraftItemRequest(goal.Id, 5, "変更しようとした")
        ]));

        Assert.False(editAfterSubmit.IsSuccess);
        Assert.Equal(409, editAfterSubmit.StatusCode);
    }

    [Fact]
    public async Task AC_SelfEval_05_評価期間外に提出しようとすると403が返る()
    {
        var period = CreateClosedPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();

        var result = await _sut.SaveDraftAsync(Guid.NewGuid(), period.Id, new SaveDraftRequest([]));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }
}
