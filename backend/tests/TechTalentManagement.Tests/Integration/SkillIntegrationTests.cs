using TechTalentManagement.Application.SkillMap;
using TechTalentManagement.Application.SkillMaster;
using TechTalentManagement.Application.UserSkills;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;
using TechTalentManagement.Infrastructure.Persistence.Repositories;

namespace TechTalentManagement.Tests.Integration;

// 受入基準: SPEC-SKILL-001 スキル管理 結合テスト
public class SkillIntegrationTests : IntegrationTestBase
{
    private readonly SkillMasterService _masterSvc;
    private readonly UserSkillService _userSkillSvc;
    private readonly SkillMapService _mapSvc;

    private readonly Guid _adminId;
    private readonly Guid _talentId;
    private readonly Guid _managerId;
    private readonly Guid _seniorId;

    public SkillIntegrationTests()
    {
        _masterSvc = new SkillMasterService(new SkillMasterRepository(Db));
        _userSkillSvc = new UserSkillService(new UserSkillRepository(Db));
        _mapSvc = new SkillMapService(new SkillMapRepository(Db));

        var admin = User.Create("oid-admin", "管理者", "admin@example.com", UserRole.Admin);
        var talent = User.Create("oid-talent", "タレント太郎", "talent@example.com", UserRole.Talent);
        var manager = User.Create("oid-manager", "上長花子", "manager@example.com", UserRole.Manager);
        var senior = User.Create("oid-senior", "管理職一郎", "senior@example.com", UserRole.Senior);

        // 部下関係: talent → manager
        var talentWithManager = User.Create("oid-talent", "タレント太郎", "talent@example.com", UserRole.Talent);

        Db.Users.AddRange(admin, manager, senior);
        Db.SaveChanges();

        // managerId を設定するため別途追加
        var talentEntry = User.Create("oid-talent2", "タレント太郎", "talent2@example.com", UserRole.Talent);
        Db.Users.Add(talentEntry);
        Db.SaveChanges();

        _adminId = admin.Id;
        _managerId = manager.Id;
        _seniorId = senior.Id;
        _talentId = talentEntry.Id;

        // ManagerId を DB 上で直接設定（EF Core で追跡中のエンティティを更新）
        var tracked = Db.Users.Find(_talentId)!;
        typeof(User).GetProperty("ManagerId")!.SetValue(tracked, _managerId);
        Db.SaveChanges();
    }

    // --- REQ-06/07: マスター管理（Admin のみ） ---

    [Fact]
    public async Task AC_REQ06_管理者がスキルカテゴリを追加できる()
    {
        var result = await _masterSvc.CreateCategoryAsync(_adminId, new CreateSkillCategoryRequest("プログラミング言語", 1));

        Assert.True(result.IsSuccess);
        Assert.Equal("プログラミング言語", result.Value!.Name);
    }

    [Fact]
    public async Task AC_REQ06_管理者以外はカテゴリを追加できない()
    {
        var result = await _masterSvc.CreateCategoryAsync(_managerId, new CreateSkillCategoryRequest("言語", 1));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task AC_REQ07_管理者がスキル種別を追加できる()
    {
        var cat = await _masterSvc.CreateCategoryAsync(_adminId, new CreateSkillCategoryRequest("言語", 1));
        var item = await _masterSvc.CreateItemAsync(_adminId, cat.Value!.Id, new CreateSkillItemRequest("C#", 1));

        Assert.True(item.IsSuccess);
        Assert.Equal("C#", item.Value!.Name);
    }

    [Fact]
    public async Task AC_REQ07_使用中スキル種別は削除できない()
    {
        var cat = await _masterSvc.CreateCategoryAsync(_adminId, new CreateSkillCategoryRequest("言語", 1));
        var item = await _masterSvc.CreateItemAsync(_adminId, cat.Value!.Id, new CreateSkillItemRequest("Go", 1));

        await _userSkillSvc.AddSkillAsync(_talentId, _talentId, new AddUserSkillRequest(item.Value!.Id, 3, null));

        var deleteResult = await _masterSvc.DeleteItemAsync(_adminId, item.Value.Id);

        Assert.False(deleteResult.IsSuccess);
        Assert.Equal(409, deleteResult.StatusCode);
    }

    // --- REQ-02: スキル追加 ---

    [Fact]
    public async Task AC_REQ02_タレントが申請中状態でスキルを追加できる()
    {
        var cat = await _masterSvc.CreateCategoryAsync(_adminId, new CreateSkillCategoryRequest("言語", 1));
        var item = await _masterSvc.CreateItemAsync(_adminId, cat.Value!.Id, new CreateSkillItemRequest("C#", 1));

        var result = await _userSkillSvc.AddSkillAsync(_talentId, _talentId,
            new AddUserSkillRequest(item.Value!.Id, 3, "実務3年"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Pending", result.Value!.Status);
        Assert.Equal(3, result.Value.Level);
    }

    [Fact]
    public async Task AC_REQ02_同一スキルを重複追加できない()
    {
        var cat = await _masterSvc.CreateCategoryAsync(_adminId, new CreateSkillCategoryRequest("言語", 1));
        var item = await _masterSvc.CreateItemAsync(_adminId, cat.Value!.Id, new CreateSkillItemRequest("Java", 1));

        await _userSkillSvc.AddSkillAsync(_talentId, _talentId, new AddUserSkillRequest(item.Value!.Id, 2, null));
        var result = await _userSkillSvc.AddSkillAsync(_talentId, _talentId, new AddUserSkillRequest(item.Value.Id, 3, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task AC_REQ02_他人のスキルを追加できない()
    {
        var cat = await _masterSvc.CreateCategoryAsync(_adminId, new CreateSkillCategoryRequest("言語", 1));
        var item = await _masterSvc.CreateItemAsync(_adminId, cat.Value!.Id, new CreateSkillItemRequest("Python", 1));

        var result = await _userSkillSvc.AddSkillAsync(_managerId, _talentId,
            new AddUserSkillRequest(item.Value!.Id, 3, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    // --- REQ-03: スキル編集 ---

    [Fact]
    public async Task AC_REQ03_編集後に申請中状態に戻る()
    {
        var cat = await _masterSvc.CreateCategoryAsync(_adminId, new CreateSkillCategoryRequest("言語", 1));
        var item = await _masterSvc.CreateItemAsync(_adminId, cat.Value!.Id, new CreateSkillItemRequest("TS", 1));
        var added = await _userSkillSvc.AddSkillAsync(_talentId, _talentId, new AddUserSkillRequest(item.Value!.Id, 2, null));

        // 一旦承認
        await ApproveSkillDirect(added.Value!.Id);

        // 編集して申請中に戻る
        var updated = await _userSkillSvc.UpdateSkillAsync(_talentId, added.Value.Id, new UpdateUserSkillRequest(4, "レベルアップ"));

        Assert.True(updated.IsSuccess);
        Assert.Equal("Pending", updated.Value!.Status);
        Assert.Equal(4, updated.Value.Level);
    }

    [Fact]
    public async Task AC_REQ03_他人のスキルを編集できない()
    {
        var cat = await _masterSvc.CreateCategoryAsync(_adminId, new CreateSkillCategoryRequest("言語", 1));
        var item = await _masterSvc.CreateItemAsync(_adminId, cat.Value!.Id, new CreateSkillItemRequest("Rust", 1));
        var added = await _userSkillSvc.AddSkillAsync(_talentId, _talentId, new AddUserSkillRequest(item.Value!.Id, 2, null));

        var result = await _userSkillSvc.UpdateSkillAsync(_managerId, added.Value!.Id, new UpdateUserSkillRequest(5, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    // --- REQ-04: スキル削除 ---

    [Fact]
    public async Task AC_REQ04_タレントが自分のスキルを削除できる()
    {
        var cat = await _masterSvc.CreateCategoryAsync(_adminId, new CreateSkillCategoryRequest("言語", 1));
        var item = await _masterSvc.CreateItemAsync(_adminId, cat.Value!.Id, new CreateSkillItemRequest("Kotlin", 1));
        var added = await _userSkillSvc.AddSkillAsync(_talentId, _talentId, new AddUserSkillRequest(item.Value!.Id, 2, null));

        var result = await _userSkillSvc.DeleteSkillAsync(_talentId, added.Value!.Id);

        Assert.True(result.IsSuccess);
        var list = await _userSkillSvc.GetUserSkillsAsync(_talentId);
        Assert.Empty(list.Value!);
    }

    // --- REQ-05: 上長承認 ---

    [Fact]
    public async Task AC_REQ05_直属上長がスキルを承認でき_承認済みになる()
    {
        var cat = await _masterSvc.CreateCategoryAsync(_adminId, new CreateSkillCategoryRequest("言語", 1));
        var item = await _masterSvc.CreateItemAsync(_adminId, cat.Value!.Id, new CreateSkillItemRequest("Swift", 1));
        var added = await _userSkillSvc.AddSkillAsync(_talentId, _talentId, new AddUserSkillRequest(item.Value!.Id, 3, null));

        var result = await _userSkillSvc.ApproveSkillAsync(_managerId, added.Value!.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("Approved", result.Value!.Status);
        Assert.Equal(_managerId, result.Value.ApprovedBy);
    }

    [Fact]
    public async Task AC_REQ05_部下以外のスキルは承認できない()
    {
        var cat = await _masterSvc.CreateCategoryAsync(_adminId, new CreateSkillCategoryRequest("言語", 1));
        var item = await _masterSvc.CreateItemAsync(_adminId, cat.Value!.Id, new CreateSkillItemRequest("Dart", 1));
        var added = await _userSkillSvc.AddSkillAsync(_talentId, _talentId, new AddUserSkillRequest(item.Value!.Id, 3, null));

        // senior は talentId の上長ではない
        var result = await _userSkillSvc.ApproveSkillAsync(_seniorId, added.Value!.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task AC_REQ05_上長が差し戻し_理由がタレントに表示される()
    {
        var cat = await _masterSvc.CreateCategoryAsync(_adminId, new CreateSkillCategoryRequest("言語", 1));
        var item = await _masterSvc.CreateItemAsync(_adminId, cat.Value!.Id, new CreateSkillItemRequest("Elixir", 1));
        var added = await _userSkillSvc.AddSkillAsync(_talentId, _talentId, new AddUserSkillRequest(item.Value!.Id, 5, null));

        var result = await _userSkillSvc.RejectSkillAsync(_managerId, added.Value!.Id, "レベル5は実績不足です");

        Assert.True(result.IsSuccess);
        Assert.Equal("Rejected", result.Value!.Status);
        Assert.Equal("レベル5は実績不足です", result.Value.RejectedReason);
    }

    [Fact]
    public async Task AC_REQ05_差し戻し理由なしは拒否される()
    {
        var cat = await _masterSvc.CreateCategoryAsync(_adminId, new CreateSkillCategoryRequest("言語", 1));
        var item = await _masterSvc.CreateItemAsync(_adminId, cat.Value!.Id, new CreateSkillItemRequest("Haskell", 1));
        var added = await _userSkillSvc.AddSkillAsync(_talentId, _talentId, new AddUserSkillRequest(item.Value!.Id, 3, null));

        var result = await _userSkillSvc.RejectSkillAsync(_managerId, added.Value!.Id, "");

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    // --- REQ-08 + REQ-10: 組織スキルマップ ---

    [Fact]
    public async Task AC_REQ08_管理職以上のみスキルマップを取得できる()
    {
        var seniorResult = await _mapSvc.GetSkillMapAsync(_seniorId);
        var talentResult = await _mapSvc.GetSkillMapAsync(_talentId);
        var managerResult = await _mapSvc.GetSkillMapAsync(_managerId);

        Assert.True(seniorResult.IsSuccess);
        Assert.False(talentResult.IsSuccess);
        Assert.Equal(403, talentResult.StatusCode);
        Assert.False(managerResult.IsSuccess);
        Assert.Equal(403, managerResult.StatusCode);
    }

    [Fact]
    public async Task AC_REQ10_承認済みスキルのみマップに反映される()
    {
        var cat = await _masterSvc.CreateCategoryAsync(_adminId, new CreateSkillCategoryRequest("言語", 1));
        var item = await _masterSvc.CreateItemAsync(_adminId, cat.Value!.Id, new CreateSkillItemRequest("C#", 1));

        var added = await _userSkillSvc.AddSkillAsync(_talentId, _talentId, new AddUserSkillRequest(item.Value!.Id, 4, null));

        // 申請中の時点ではマップに出ない
        var mapBeforeApprove = await _mapSvc.GetSkillMapAsync(_seniorId);
        Assert.True(mapBeforeApprove.IsSuccess);
        Assert.Empty(mapBeforeApprove.Value!.Categories);

        // 承認後はマップに出る
        await _userSkillSvc.ApproveSkillAsync(_managerId, added.Value!.Id);
        var mapAfterApprove = await _mapSvc.GetSkillMapAsync(_seniorId);
        Assert.True(mapAfterApprove.IsSuccess);
        Assert.Single(mapAfterApprove.Value!.Categories);
        Assert.Equal(4, mapAfterApprove.Value.Categories[0].Items[0].UserLevels[0].Level);
    }

    private async Task ApproveSkillDirect(Guid userSkillId)
    {
        var skill = await Db.UserSkills.FindAsync(userSkillId);
        skill!.Approve(_managerId);
        await Db.SaveChangesAsync();
    }
}
