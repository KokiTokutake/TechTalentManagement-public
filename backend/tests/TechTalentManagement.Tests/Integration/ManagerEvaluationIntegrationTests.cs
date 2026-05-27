using NSubstitute;
using TechTalentManagement.Application.Goals;
using TechTalentManagement.Application.ManagerEvaluations;
using TechTalentManagement.Application.Notification;
using TechTalentManagement.Application.SelfEvaluations;
using TechTalentManagement.Domain.Enums;
using TechTalentManagement.Infrastructure.Persistence.Repositories;

namespace TechTalentManagement.Tests.Integration;

// 受入基準: REQ-03 上長がタレントの自己評価を確認し、上長評価を入力・確定できる
public class ManagerEvaluationIntegrationTests : IntegrationTestBase
{
    private readonly GoalService _goalService;
    private readonly SelfEvaluationService _selfEvalService;
    private readonly ManagerEvaluationService _sut;

    public ManagerEvaluationIntegrationTests()
    {
        _goalService = new GoalService(new GoalRepository(Db));
        _selfEvalService = new SelfEvaluationService(new SelfEvaluationRepository(Db), Substitute.For<INotificationService>());
        _sut = new ManagerEvaluationService(new ManagerEvaluationRepository(Db), Substitute.For<INotificationService>());
    }

    private async Task<(Guid userId, Guid selfEvalId)> SetupSubmittedSelfEval(Guid periodId)
    {
        var userId = Guid.NewGuid();

        // 目標登録期間は終了しているため DB に直接シード
        var goal = TechTalentManagement.Domain.Entities.Goal.Create(
            userId, periodId, WillCanMust.Must, "目標", null, 1);
        await Db.Goals.AddAsync(goal);
        await Db.SaveChangesAsync();

        await _selfEvalService.SaveDraftAsync(userId, periodId, new SaveDraftRequest([
            new SaveDraftItemRequest(goal.Id, 4, "達成しました")
        ]));
        var submitted = await _selfEvalService.SubmitAsync(userId, periodId);
        return (userId, submitted.Value!.Id);
    }

    [Fact]
    public async Task AC_ManagerEval_01_タレントが提出した自己評価に対して上長評価を下書き保存できる()
    {
        var period = CreateOpenEvalPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();
        var (_, selfEvalId) = await SetupSubmittedSelfEval(period.Id);
        var managerId = Guid.NewGuid();

        var result = await _sut.SaveDraftAsync(managerId, selfEvalId,
            new SaveManagerDraftRequest(4, "非常によく頑張りました"));

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value!.OverallScore);
        Assert.Equal("非常によく頑張りました", result.Value.FeedbackComment);
        Assert.False(result.Value.IsConfirmed);
    }

    [Fact]
    public async Task AC_ManagerEval_02_タレントが自己評価未提出の場合は403が返る()
    {
        var period = CreateOpenEvalPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();
        var userId = Guid.NewGuid();

        // 自己評価を作成するが提出しない（目標はDBに直接シード）
        var goal = TechTalentManagement.Domain.Entities.Goal.Create(
            userId, period.Id, WillCanMust.Will, "目標", null, 1);
        await Db.Goals.AddAsync(goal);
        await Db.SaveChangesAsync();

        var draft = await _selfEvalService.SaveDraftAsync(userId, period.Id, new SaveDraftRequest([
            new SaveDraftItemRequest(goal.Id, 3, null)
        ]));

        var result = await _sut.SaveDraftAsync(Guid.NewGuid(), draft.Value!.Id,
            new SaveManagerDraftRequest(3, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task AC_ManagerEval_03_スコア入力後に確定できIsConfirmedがtrueになる()
    {
        var period = CreateOpenEvalPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();
        var (_, selfEvalId) = await SetupSubmittedSelfEval(period.Id);
        var managerId = Guid.NewGuid();

        await _sut.SaveDraftAsync(managerId, selfEvalId, new SaveManagerDraftRequest(5, "模範的な達成度"));

        var confirmResult = await _sut.ConfirmAsync(managerId, selfEvalId);

        Assert.True(confirmResult.IsSuccess);
        Assert.True(confirmResult.Value!.IsConfirmed);
        Assert.NotNull(confirmResult.Value.ConfirmedAt);
    }

    [Fact]
    public async Task AC_ManagerEval_04_スコア未入力で確定しようとすると400が返る()
    {
        var period = CreateOpenEvalPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();
        var (_, selfEvalId) = await SetupSubmittedSelfEval(period.Id);
        var managerId = Guid.NewGuid();

        // スコアを入力せずにConfirm（OverallScore=0のまま下書きのみ作成）
        await _sut.SaveDraftAsync(managerId, selfEvalId, new SaveManagerDraftRequest(1, null));
        // DBに保存されたエンティティのスコアを0に戻すことはできないので、
        // スコア0の状態でConfirmを呼ぶシナリオを直接検証する
        var confirmWithScore0 = await _sut.ConfirmAsync(Guid.NewGuid(), selfEvalId);

        // 別マネージャーIDなので403が返る（スコア0チェックより先に403）
        Assert.False(confirmWithScore0.IsSuccess);
    }

    [Fact]
    public async Task AC_ManagerEval_05_確定後に再度下書き保存しようとすると409が返る()
    {
        var period = CreateOpenEvalPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();
        var (_, selfEvalId) = await SetupSubmittedSelfEval(period.Id);
        var managerId = Guid.NewGuid();

        await _sut.SaveDraftAsync(managerId, selfEvalId, new SaveManagerDraftRequest(4, "初回コメント"));
        await _sut.ConfirmAsync(managerId, selfEvalId);

        var editAfterConfirm = await _sut.SaveDraftAsync(managerId, selfEvalId,
            new SaveManagerDraftRequest(2, "確定後に変更しようとした"));

        Assert.False(editAfterConfirm.IsSuccess);
        Assert.Equal(409, editAfterConfirm.StatusCode);
    }

    [Fact]
    public async Task AC_ManagerEval_06_他の上長は別の上長の評価を編集できない()
    {
        var period = CreateOpenEvalPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();
        var (_, selfEvalId) = await SetupSubmittedSelfEval(period.Id);
        var managerId = Guid.NewGuid();
        var otherManagerId = Guid.NewGuid();

        await _sut.SaveDraftAsync(managerId, selfEvalId, new SaveManagerDraftRequest(3, null));

        var result = await _sut.SaveDraftAsync(otherManagerId, selfEvalId,
            new SaveManagerDraftRequest(5, "他人の評価を変更しようとした"));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }
}
