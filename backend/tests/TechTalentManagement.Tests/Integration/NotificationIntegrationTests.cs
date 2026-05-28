using NSubstitute;
using TechTalentManagement.Application.Goals;
using TechTalentManagement.Application.ManagerEvaluations;
using TechTalentManagement.Application.Notification;
using TechTalentManagement.Application.SelfEvaluations;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;
using TechTalentManagement.Infrastructure.Persistence.Repositories;

namespace TechTalentManagement.Tests.Integration;

// SPEC-NOTIF-001: 通知・アラートシステム 結合テスト（TASK-088）
// 受入基準: AC-01-01〜AC-04-03 全通知 CRUD + トリガー通知生成
public class NotificationIntegrationTests : IntegrationTestBase
{
    private readonly NotificationService _sut;
    private readonly NotificationRepository _notifRepo;

    public NotificationIntegrationTests()
    {
        _notifRepo = new NotificationRepository(Db);
        _sut = new NotificationService(_notifRepo);
    }

    // ── ヘルパー ────────────────────────────────────────────────────────────

    private async Task SeedNotification(Guid userId, string title, bool isRead = false)
    {
        await _sut.CreateAsync(userId, NotificationType.EvaluationApproved, title, "本文");
        if (isRead)
        {
            var notif = Db.Notifications.First(n => n.Title == title);
            notif.MarkAsRead();
            await Db.SaveChangesAsync();
        }
    }

    // ── AC-01-01: 未読バッジ表示 ────────────────────────────────────────────

    [Fact]
    public async Task AC_Notif_01_01_未読通知が3件あるとき未読数3が返る()
    {
        var userId = Guid.NewGuid();
        await SeedNotification(userId, "通知1");
        await SeedNotification(userId, "通知2");
        await SeedNotification(userId, "通知3");

        var result = await _sut.GetUnreadCountAsync(userId, userId);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
    }

    // ── AC-01-02: 未読0件でバッジ非表示 ─────────────────────────────────────

    [Fact]
    public async Task AC_Notif_01_02_未読通知が0件のとき未読数0が返る()
    {
        var userId = Guid.NewGuid();

        var result = await _sut.GetUnreadCountAsync(userId, userId);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.Count);
    }

    [Fact]
    public async Task AC_Notif_01_02_全通知既読済みのとき未読数0が返る()
    {
        var userId = Guid.NewGuid();
        await SeedNotification(userId, "既読通知", isRead: true);

        var result = await _sut.GetUnreadCountAsync(userId, userId);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.Count);
    }

    // ── AC-02-01: 通知一覧表示（新着順・既読区別） ──────────────────────────

    [Fact]
    public async Task AC_Notif_02_01_通知一覧が新着順で返りisReadフラグが区別される()
    {
        var userId = Guid.NewGuid();
        await SeedNotification(userId, "古い通知");
        await Task.Delay(5); // CreatedAt の差を確保
        await SeedNotification(userId, "新しい通知");
        await SeedNotification(userId, "既読通知", isRead: true);

        var result = await _sut.GetListAsync(userId, userId, unreadOnly: false);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
        // 新着順確認
        Assert.True(result.Value[0].CreatedAt >= result.Value[1].CreatedAt);
        // isRead 区別確認
        var unread = result.Value.Where(n => !n.IsRead).ToList();
        var read = result.Value.Where(n => n.IsRead).ToList();
        Assert.Equal(2, unread.Count);
        Assert.Single(read);
    }

    [Fact]
    public async Task AC_Notif_02_01_unreadOnly指定で未読のみ返る()
    {
        var userId = Guid.NewGuid();
        await SeedNotification(userId, "未読通知");
        await SeedNotification(userId, "既読通知", isRead: true);

        var result = await _sut.GetListAsync(userId, userId, unreadOnly: true);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.False(result.Value![0].IsRead);
    }

    // ── AC-03-01: 通知クリックで既読化 ──────────────────────────────────────

    [Fact]
    public async Task AC_Notif_03_01_通知を既読化するとIsReadがtrueになり未読数が減る()
    {
        var userId = Guid.NewGuid();
        await SeedNotification(userId, "既読化テスト通知");
        var notifId = Db.Notifications.First(n => n.UserId == userId).Id;

        var markResult = await _sut.MarkAsReadAsync(userId, notifId);
        var countResult = await _sut.GetUnreadCountAsync(userId, userId);

        Assert.True(markResult.IsSuccess);
        Assert.True(markResult.Value!.IsRead);
        Assert.NotNull(markResult.Value.ReadAt);
        Assert.Equal(0, countResult.Value!.Count);
    }

    [Fact]
    public async Task AC_Notif_03_01_存在しない通知の既読化は404を返す()
    {
        var result = await _sut.MarkAsReadAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task AC_Notif_03_01_他ユーザーの通知の既読化は403を返す()
    {
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        await SeedNotification(ownerId, "他人の通知");
        var notifId = Db.Notifications.First(n => n.UserId == ownerId).Id;

        var result = await _sut.MarkAsReadAsync(attackerId, notifId);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    // ── AC-03-02: 一括既読 ────────────────────────────────────────────────

    [Fact]
    public async Task AC_Notif_03_02_一括既読化すると全通知がIsReadになりバッジが消える()
    {
        var userId = Guid.NewGuid();
        await SeedNotification(userId, "未読1");
        await SeedNotification(userId, "未読2");
        await SeedNotification(userId, "未読3");

        var markAllResult = await _sut.MarkAllAsReadAsync(userId, userId);
        var countResult = await _sut.GetUnreadCountAsync(userId, userId);

        Assert.True(markAllResult.IsSuccess);
        Assert.Equal(3, markAllResult.Value!.UpdatedCount);
        Assert.Equal(0, countResult.Value!.Count);
    }

    [Fact]
    public async Task AC_Notif_03_02_未読0件での一括既読はupdatedCount0を返す()
    {
        var userId = Guid.NewGuid();

        var result = await _sut.MarkAllAsReadAsync(userId, userId);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.UpdatedCount);
    }

    // ── 認可チェック ──────────────────────────────────────────────────────

    [Fact]
    public async Task 他ユーザーの通知一覧取得は403を返す()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();

        var result = await _sut.GetListAsync(requesterId, ownerId, unreadOnly: false);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task 他ユーザーの未読数取得は403を返す()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();

        var result = await _sut.GetUnreadCountAsync(requesterId, ownerId);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    // ── AC-04-01: トリガー別通知生成（NT-03: 自己評価提出） ──────────────────

    [Fact]
    public async Task NT_03_自己評価提出時に上長への通知が生成される()
    {
        // Arrange: 上長とタレントを DB にシード
        var managerId = Guid.NewGuid();
        var manager = User.Create("manager-oid", "上長 花子", "manager@test.com", UserRole.Manager);
        await Db.Users.AddAsync(manager);

        var talent = User.Create("talent-oid", "タレント 太郎", "talent@test.com", UserRole.Talent, manager.Id);
        await Db.Users.AddAsync(talent);
        await Db.SaveChangesAsync();

        var period = CreateOpenEvalPeriod();
        await Db.EvaluationPeriods.AddAsync(period);

        var goal = Goal.Create(talent.Id, period.Id, WillCanMust.Must, "目標", null, 1);
        await Db.Goals.AddAsync(goal);
        await Db.SaveChangesAsync();

        // 実際の NotificationService で SelfEvaluationService を構成
        var selfEvalService = new SelfEvaluationService(new SelfEvaluationRepository(Db), _sut);
        await selfEvalService.SaveDraftAsync(talent.Id, period.Id, new SaveDraftRequest([
            new SaveDraftItemRequest(goal.Id, 4, "達成")
        ]));

        // Act
        var submitResult = await selfEvalService.SubmitAsync(talent.Id, period.Id);

        // Assert: 上長に通知が届いた
        Assert.True(submitResult.IsSuccess);
        var managerCount = await _sut.GetUnreadCountAsync(manager.Id, manager.Id);
        Assert.Equal(1, managerCount.Value!.Count);

        var managerNotifs = await _sut.GetListAsync(manager.Id, manager.Id, unreadOnly: false);
        Assert.Single(managerNotifs.Value!);
        Assert.Equal(nameof(NotificationType.SelfEvaluationSubmitted), managerNotifs.Value![0].Type);
    }

    // ── AC-04-01: トリガー別通知生成（NT-05: 上長評価確定） ──────────────────

    [Fact]
    public async Task NT_05_上長評価確定時にタレントへの通知が生成される()
    {
        // Arrange
        var period = CreateOpenEvalPeriod();
        await Db.EvaluationPeriods.AddAsync(period);
        await Db.SaveChangesAsync();

        var talent = User.Create("talent-oid2", "タレント 次郎", "talent2@test.com", UserRole.Talent);
        await Db.Users.AddAsync(talent);

        var goal = Goal.Create(talent.Id, period.Id, WillCanMust.Must, "目標2", null, 1);
        await Db.Goals.AddAsync(goal);
        await Db.SaveChangesAsync();

        var selfEvalService = new SelfEvaluationService(
            new SelfEvaluationRepository(Db), Substitute.For<INotificationService>());
        await selfEvalService.SaveDraftAsync(talent.Id, period.Id, new SaveDraftRequest([
            new SaveDraftItemRequest(goal.Id, 3, null)
        ]));
        var submitted = await selfEvalService.SubmitAsync(talent.Id, period.Id);
        var selfEvalId = submitted.Value!.Id;

        // 実際の NotificationService で ManagerEvaluationService を構成
        var managerId = Guid.NewGuid();
        var managerEvalService = new ManagerEvaluationService(new ManagerEvaluationRepository(Db), _sut);
        await managerEvalService.SaveDraftAsync(managerId, selfEvalId, new SaveManagerDraftRequest(4, "よく頑張りました"));

        // Act
        var confirmResult = await managerEvalService.ConfirmAsync(managerId, selfEvalId);

        // Assert: タレントに通知が届いた
        Assert.True(confirmResult.IsSuccess);
        var talentCount = await _sut.GetUnreadCountAsync(talent.Id, talent.Id);
        Assert.Equal(1, talentCount.Value!.Count);

        var talentNotifs = await _sut.GetListAsync(talent.Id, talent.Id, unreadOnly: false);
        Assert.Single(talentNotifs.Value!);
        Assert.Equal(nameof(NotificationType.EvaluationApproved), talentNotifs.Value![0].Type);
    }

    // ── AC-04-01: EvaluationRejected 通知インフラ（NT-04 差し戻しトリガーは v1 未実装） ──

    [Fact]
    public async Task AC_Notif_04_01_差し戻し通知がCreateAsyncで生成されGetListAsyncで取得できる()
    {
        var userId = Guid.NewGuid();

        await _sut.CreateAsync(
            userId,
            NotificationType.EvaluationRejected,
            "上長評価で差し戻されました",
            "目標設定を修正して再提出してください。",
            linkUrl: "/evaluation/self");

        var listResult = await _sut.GetListAsync(userId, userId, unreadOnly: false);

        Assert.True(listResult.IsSuccess);
        Assert.Single(listResult.Value!);
        Assert.Equal("上長評価で差し戻されました", listResult.Value![0].Title);
        Assert.Equal(nameof(NotificationType.EvaluationRejected), listResult.Value![0].Type);
        Assert.Equal("/evaluation/self", listResult.Value![0].LinkUrl);
        Assert.False(listResult.Value![0].IsRead);
    }

    // ── AC-04-02: SuccessionGapAlert 通知インフラ（NT-08 自動検知は v1 未実装） ──

    [Fact]
    public async Task AC_Notif_04_02_後継者ギャップアラートがCreateAsyncで生成されGetListAsyncで取得できる()
    {
        var seniorId = Guid.NewGuid();

        await _sut.CreateAsync(
            seniorId,
            NotificationType.SuccessionGapAlert,
            "後継者未定アラート",
            "重要ポジション「CTO」に Ready Now の後継者候補がいません。");

        var listResult = await _sut.GetListAsync(seniorId, seniorId, unreadOnly: false);

        Assert.True(listResult.IsSuccess);
        Assert.Single(listResult.Value!);
        Assert.Equal(nameof(NotificationType.SuccessionGapAlert), listResult.Value![0].Type);
        Assert.False(listResult.Value![0].IsRead);
    }

    // ── AC-04-03: NineBoxKeyPersonCandidate 通知（NT-07） ───────────────────

    [Fact]
    public async Task AC_Notif_04_03_重点管理候補フラグ通知がCreateAsyncで生成されGetListAsyncで取得できる()
    {
        var talentId = Guid.NewGuid();

        await _sut.CreateAsync(
            talentId,
            NotificationType.NineBoxKeyPersonCandidate,
            "重点管理人物候補に選出されました",
            "9ボックス評価の結果、あなたが重点管理人物候補に選出されました。");

        var countResult = await _sut.GetUnreadCountAsync(talentId, talentId);
        var listResult = await _sut.GetListAsync(talentId, talentId, unreadOnly: false);

        Assert.True(countResult.IsSuccess);
        Assert.Equal(1, countResult.Value!.Count);
        Assert.Equal(nameof(NotificationType.NineBoxKeyPersonCandidate), listResult.Value![0].Type);
    }
}
