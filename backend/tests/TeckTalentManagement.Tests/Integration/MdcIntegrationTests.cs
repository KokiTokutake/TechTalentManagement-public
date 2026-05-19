using TeckTalentManagement.Application.CommitteeSession;
using TeckTalentManagement.Application.DevelopmentPlan;
using TeckTalentManagement.Application.KeyPerson;
using TeckTalentManagement.Domain.Entities;
using TeckTalentManagement.Domain.Enums;
using TeckTalentManagement.Infrastructure.Persistence.Repositories;

namespace TeckTalentManagement.Tests.Integration;

// 受入基準: SPEC-MDC-001 人材開発委員会機能 結合テスト
public class MdcIntegrationTests : IntegrationTestBase
{
    private readonly KeyPersonService _keyPersonSvc;
    private readonly DevelopmentPlanService _devPlanSvc;
    private readonly CommitteeSessionService _sessionSvc;

    private readonly Guid _talentId;
    private readonly Guid _managerId;
    private readonly Guid _seniorId;
    private readonly Guid _talent2Id;
    private readonly Guid _manager2Id;

    public MdcIntegrationTests()
    {
        _keyPersonSvc = new KeyPersonService(new KeyPersonRepository(Db));
        _devPlanSvc = new DevelopmentPlanService(new DevelopmentPlanRepository(Db));
        _sessionSvc = new CommitteeSessionService(new CommitteeSessionRepository(Db));

        var talent = User.Create("oid-talent", "タレント太郎", "talent@example.com", UserRole.Talent);
        var manager = User.Create("oid-manager", "上長花子", "manager@example.com", UserRole.Manager);
        var senior = User.Create("oid-senior", "管理職一郎", "senior@example.com", UserRole.Senior);
        var talent2 = User.Create("oid-talent2", "タレント次郎", "talent2@example.com", UserRole.Talent);
        var manager2 = User.Create("oid-manager2", "上長二郎", "manager2@example.com", UserRole.Manager);

        Db.Users.AddRange(talent, manager, senior, talent2, manager2);
        Db.SaveChanges();

        _talentId = talent.Id;
        _managerId = manager.Id;
        _seniorId = senior.Id;
        _talent2Id = talent2.Id;
        _manager2Id = manager2.Id;

        // talent の直属上長 = manager
        var trackedTalent = Db.Users.Find(_talentId)!;
        typeof(User).GetProperty("ManagerId")!.SetValue(trackedTalent, _managerId);
        // talent2 の直属上長 = manager2
        var trackedTalent2 = Db.Users.Find(_talent2Id)!;
        typeof(User).GetProperty("ManagerId")!.SetValue(trackedTalent2, _manager2Id);
        Db.SaveChanges();
    }

    // ===== REQ-01: 重点管理人物の指定・管理 =====

    [Fact]
    public async Task AC_01_01_Seniorが重点管理人物を指定できる()
    {
        var result = await _keyPersonSvc.DesignateAsync(
            _seniorId, new DesignateKeyPersonRequest(_talentId, "将来有望なエンジニア"));

        Assert.True(result.IsSuccess);
        Assert.Equal(_talentId, result.Value!.TalentId);
        Assert.Equal(_seniorId, result.Value.DesignatedBy);
        Assert.True(result.Value.IsActive);
        Assert.Equal("将来有望なエンジニア", result.Value.Reason);
    }

    [Fact]
    public async Task AC_01_02_同一タレントへの重複指定は409()
    {
        await _keyPersonSvc.DesignateAsync(_seniorId, new DesignateKeyPersonRequest(_talentId, null));

        var result = await _keyPersonSvc.DesignateAsync(_seniorId, new DesignateKeyPersonRequest(_talentId, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task AC_01_03_Seniorが重点管理人物を解除できる()
    {
        var designated = await _keyPersonSvc.DesignateAsync(_seniorId, new DesignateKeyPersonRequest(_talentId, null));
        var keyPersonId = designated.Value!.Id;

        var revokeResult = await _keyPersonSvc.RevokeAsync(_seniorId, keyPersonId);

        Assert.True(revokeResult.IsSuccess);

        // 解除後は一覧に表示されない
        var list = await _keyPersonSvc.GetListAsync(_seniorId);
        Assert.True(list.IsSuccess);
        Assert.Empty(list.Value!);

        // タレント本人のページにも表示されない
        var myStatus = await _keyPersonSvc.GetMyStatusAsync(_talentId);
        Assert.True(myStatus.IsSuccess);
        Assert.Null(myStatus.Value);
    }

    [Fact]
    public async Task AC_01_04_TalentとManagerは指定できない()
    {
        var talentResult = await _keyPersonSvc.DesignateAsync(
            _talentId, new DesignateKeyPersonRequest(_talent2Id, null));
        var managerResult = await _keyPersonSvc.DesignateAsync(
            _managerId, new DesignateKeyPersonRequest(_talent2Id, null));

        Assert.False(talentResult.IsSuccess);
        Assert.Equal(403, talentResult.StatusCode);
        Assert.False(managerResult.IsSuccess);
        Assert.Equal(403, managerResult.StatusCode);
    }

    [Fact]
    public async Task AC_01_05_タレント本人が指定状況を取得できる()
    {
        await _keyPersonSvc.DesignateAsync(_seniorId, new DesignateKeyPersonRequest(_talentId, "高評価"));

        var result = await _keyPersonSvc.GetMyStatusAsync(_talentId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(_talentId, result.Value!.TalentId);
        Assert.True(result.Value.IsActive);
        Assert.Equal("高評価", result.Value.Reason);
    }

    // ===== REQ-02: 育成方針・アクションプランの記録 =====

    [Fact]
    public async Task AC_02_01_直属ManagerがタレントへDevelopmentPlanを登録できる()
    {
        var kp = await _keyPersonSvc.DesignateAsync(_seniorId, new DesignateKeyPersonRequest(_talentId, null));

        var result = await _devPlanSvc.AddAsync(_managerId, kp.Value!.Id,
            new SaveDevelopmentPlanRequest("課題A", "目標A", "サポートA", null));

        Assert.True(result.IsSuccess);
        Assert.Equal("課題A", result.Value!.Challenge);
        Assert.Equal("目標A", result.Value.Goal);
        Assert.Equal("サポートA", result.Value.SupportAction);
        Assert.Equal(1, result.Value.Version);
    }

    [Fact]
    public async Task AC_02_02_直属Manager以外は書き込めない()
    {
        var kp = await _keyPersonSvc.DesignateAsync(_seniorId, new DesignateKeyPersonRequest(_talentId, null));

        // senior は manager ではないので 403
        var result = await _devPlanSvc.AddAsync(_seniorId, kp.Value!.Id,
            new SaveDevelopmentPlanRequest("課題", "目標", "サポート", null));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task AC_02_03_育成方針の更新で新バージョンが追加される()
    {
        var kp = await _keyPersonSvc.DesignateAsync(_seniorId, new DesignateKeyPersonRequest(_talentId, null));

        await _devPlanSvc.AddAsync(_managerId, kp.Value!.Id,
            new SaveDevelopmentPlanRequest("課題v1", "目標v1", "サポートv1", null));
        var v2 = await _devPlanSvc.AddAsync(_managerId, kp.Value.Id,
            new SaveDevelopmentPlanRequest("課題v2", "目標v2", "サポートv2", null));

        Assert.True(v2.IsSuccess);
        Assert.Equal(2, v2.Value!.Version);

        var list = await _devPlanSvc.GetListAsync(_managerId, kp.Value.Id);
        Assert.Equal(2, list.Value!.Count);
    }

    [Fact]
    public async Task AC_02_04_タレント本人が育成方針を閲覧できる()
    {
        var kp = await _keyPersonSvc.DesignateAsync(_seniorId, new DesignateKeyPersonRequest(_talentId, null));
        await _devPlanSvc.AddAsync(_managerId, kp.Value!.Id,
            new SaveDevelopmentPlanRequest("課題", "目標", "サポート", null));

        var result = await _devPlanSvc.GetListAsync(_talentId, kp.Value.Id);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("課題", result.Value![0].Challenge);
    }

    [Fact]
    public async Task AC_02_05_全フィールド空の育成方針は保存できない()
    {
        var kp = await _keyPersonSvc.DesignateAsync(_seniorId, new DesignateKeyPersonRequest(_talentId, null));

        var result = await _devPlanSvc.AddAsync(_managerId, kp.Value!.Id,
            new SaveDevelopmentPlanRequest("", "", "", null));

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task AC_02_06_解除済みKeyPersonへの育成方針追加は409()
    {
        var kp = await _keyPersonSvc.DesignateAsync(_seniorId, new DesignateKeyPersonRequest(_talentId, null));
        await _keyPersonSvc.RevokeAsync(_seniorId, kp.Value!.Id);

        var result = await _devPlanSvc.AddAsync(_managerId, kp.Value.Id,
            new SaveDevelopmentPlanRequest("課題", "目標", "サポート", null));

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.StatusCode);
    }

    // ===== REQ-03: 委員会セッション管理 =====

    [Fact]
    public async Task AC_03_01_Seniorが委員会セッションを作成できる()
    {
        var kp = await _keyPersonSvc.DesignateAsync(_seniorId, new DesignateKeyPersonRequest(_talentId, null));
        var heldOn = DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await _sessionSvc.CreateAsync(_seniorId, new CreateCommitteeSessionRequest(
            heldOn, "議事録テスト",
            [_seniorId, _managerId],
            [kp.Value!.Id]
        ));

        Assert.True(result.IsSuccess);
        Assert.Equal(heldOn, result.Value!.HeldOn);
        Assert.Equal("議事録テスト", result.Value.Minutes);
        Assert.Equal(2, result.Value.ParticipantIds.Count);
        Assert.Single(result.Value.KeyPersonIds);
    }

    [Fact]
    public async Task AC_03_02_TalentとManagerはセッション作成できない()
    {
        var heldOn = DateOnly.FromDateTime(DateTime.UtcNow);
        var req = new CreateCommitteeSessionRequest(heldOn, "議事録", [], []);

        var talentResult = await _sessionSvc.CreateAsync(_talentId, req);
        var managerResult = await _sessionSvc.CreateAsync(_managerId, req);

        Assert.False(talentResult.IsSuccess);
        Assert.Equal(403, talentResult.StatusCode);
        Assert.False(managerResult.IsSuccess);
        Assert.Equal(403, managerResult.StatusCode);
    }

    [Fact]
    public async Task AC_03_03_Managerは担当部下が対象のセッションのみ閲覧できる()
    {
        // talent → manager, talent2 → manager2
        var kp1 = await _keyPersonSvc.DesignateAsync(_seniorId, new DesignateKeyPersonRequest(_talentId, null));
        var kp2 = await _keyPersonSvc.DesignateAsync(_seniorId, new DesignateKeyPersonRequest(_talent2Id, null));

        var heldOn = DateOnly.FromDateTime(DateTime.UtcNow);

        // セッション1: talent が対象 (manager の部下)
        await _sessionSvc.CreateAsync(_seniorId, new CreateCommitteeSessionRequest(
            heldOn, "セッション1", [], [kp1.Value!.Id]));

        // セッション2: talent2 が対象 (manager2 の部下、manager には関係ない)
        await _sessionSvc.CreateAsync(_seniorId, new CreateCommitteeSessionRequest(
            heldOn, "セッション2", [], [kp2.Value!.Id]));

        var managerResult = await _sessionSvc.GetListAsync(_managerId);

        Assert.True(managerResult.IsSuccess);
        Assert.Single(managerResult.Value!);
        Assert.Equal("セッション1", managerResult.Value![0].Minutes);
    }

    [Fact]
    public async Task AC_03_04_未来日付のセッションも作成できる()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));

        var result = await _sessionSvc.CreateAsync(_seniorId, new CreateCommitteeSessionRequest(
            futureDate, "事前計画", [], []));

        Assert.True(result.IsSuccess);
        Assert.Equal(futureDate, result.Value!.HeldOn);
    }

    // ===== REQ-04: 委員会向けサマリービュー =====

    [Fact]
    public async Task AC_04_01_Seniorがサマリーを取得できる()
    {
        await _keyPersonSvc.DesignateAsync(_seniorId, new DesignateKeyPersonRequest(_talentId, null));

        var result = await _sessionSvc.GetCommitteeSummaryAsync(_seniorId);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(_talentId, result.Value![0].TalentId);
        Assert.Equal("タレント太郎", result.Value[0].TalentName);
    }

    [Fact]
    public async Task AC_04_02_TalentとManagerはサマリーにアクセスできない()
    {
        var talentResult = await _sessionSvc.GetCommitteeSummaryAsync(_talentId);
        var managerResult = await _sessionSvc.GetCommitteeSummaryAsync(_managerId);

        Assert.False(talentResult.IsSuccess);
        Assert.Equal(403, talentResult.StatusCode);
        Assert.False(managerResult.IsSuccess);
        Assert.Equal(403, managerResult.StatusCode);
    }

    [Fact]
    public async Task AC_04_01_サマリーに育成方針登録状況が反映される()
    {
        var kp = await _keyPersonSvc.DesignateAsync(_seniorId, new DesignateKeyPersonRequest(_talentId, null));
        await _devPlanSvc.AddAsync(_managerId, kp.Value!.Id,
            new SaveDevelopmentPlanRequest("課題", "目標", "サポート", null));

        var result = await _sessionSvc.GetCommitteeSummaryAsync(_seniorId);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value![0].HasDevelopmentPlan);
    }
}
