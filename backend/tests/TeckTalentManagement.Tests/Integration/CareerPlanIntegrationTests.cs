using TeckTalentManagement.Application.CareerPlan;
using TeckTalentManagement.Domain.Entities;
using TeckTalentManagement.Domain.Enums;
using TeckTalentManagement.Infrastructure.Persistence.Repositories;

namespace TeckTalentManagement.Tests.Integration;

// 受入基準: SPEC-CAREER-001 キャリアプラン管理 結合テスト
public class CareerPlanIntegrationTests : IntegrationTestBase
{
    private readonly CareerPlanService _svc;
    private readonly Guid _talentId;
    private readonly Guid _managerId;
    private readonly Guid _seniorId;
    private readonly Guid _otherTalentId;

    public CareerPlanIntegrationTests()
    {
        _svc = new CareerPlanService(new CareerPlanRepository(Db));

        var talent = User.Create("oid-talent", "タレント太郎", "talent@example.com", UserRole.Talent);
        var manager = User.Create("oid-manager", "上長花子", "manager@example.com", UserRole.Manager);
        var senior = User.Create("oid-senior", "管理職一郎", "senior@example.com", UserRole.Senior);
        var otherTalent = User.Create("oid-other", "他のタレント", "other@example.com", UserRole.Talent);

        Db.Users.AddRange(talent, manager, senior, otherTalent);
        Db.SaveChanges();

        _talentId = talent.Id;
        _managerId = manager.Id;
        _seniorId = senior.Id;
        _otherTalentId = otherTalent.Id;

        // talent → manager の部下関係を設定
        var tracked = Db.Users.Find(_talentId)!;
        typeof(User).GetProperty("ManagerId")!.SetValue(tracked, _managerId);
        Db.SaveChanges();
    }

    // --- REQ-01: キャリアプランの作成・編集 ---

    [Fact]
    public async Task AC01_01_タレントが今期プランを新規作成できる()
    {
        var req = new SaveCareerPlanRequest("10年後にテックリードになる", "3年でシステム設計力を高める", "設計スキル向上");

        var result = await _svc.CreateAsync(_talentId, _talentId, req);

        Assert.True(result.IsSuccess);
        Assert.Equal(DateTime.UtcNow.Year, result.Value!.PeriodYear);
        Assert.Equal("10年後にテックリードになる", result.Value.LongTermGoal);
        Assert.Equal("設計スキル向上", result.Value.CurrentFocus);
    }

    [Fact]
    public async Task AC01_01_同じ年度のプランは重複作成できない_409()
    {
        await _svc.CreateAsync(_talentId, _talentId, new SaveCareerPlanRequest("目標A", "中期A", "注力A"));

        var result = await _svc.CreateAsync(_talentId, _talentId, new SaveCareerPlanRequest("目標B", "中期B", "注力B"));

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task AC01_01_他人のプランは作成できない_403()
    {
        var result = await _svc.CreateAsync(_managerId, _talentId, new SaveCareerPlanRequest("目標", "中期", "注力"));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task AC01_03_保存済みキャリアプランを編集できる()
    {
        await _svc.CreateAsync(_talentId, _talentId, new SaveCareerPlanRequest("旧目標", "旧中期", "旧注力"));
        var created = (await _svc.GetAsync(_talentId, _talentId, null)).Value!;

        var result = await _svc.UpdateAsync(_talentId, created.Id, new SaveCareerPlanRequest("新目標", "新中期", "新注力"));

        Assert.True(result.IsSuccess);
        Assert.Equal("新目標", result.Value!.LongTermGoal);
        Assert.Equal("新注力", result.Value.CurrentFocus);
    }

    [Fact]
    public async Task AC01_04_過去の期のプランは編集できない_403()
    {
        var pastPlan = CareerPlan.Create(_talentId, "過去目標", "過去中期", "過去注力");
        typeof(CareerPlan).GetProperty("PeriodYear")!.SetValue(pastPlan, DateTime.UtcNow.Year - 1);
        await Db.CareerPlans.AddAsync(pastPlan);
        await Db.SaveChangesAsync();

        var result = await _svc.UpdateAsync(_talentId, pastPlan.Id, new SaveCareerPlanRequest("更新目標", "更新中期", "更新注力"));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    // --- REQ-02: Will-Can-Must 統合ビュー ---

    [Fact]
    public async Task AC02_01_統合ビューにWillCanMustが表示される()
    {
        // Will
        await _svc.CreateAsync(_talentId, _talentId, new SaveCareerPlanRequest("長期目標", "中期目標", "注力テーマ"));

        // Can: 承認済みスキル
        var cat = SkillCategory.Create("言語", 1);
        var item = SkillItem.Create(cat.Id, "C#", 1);
        Db.SkillCategories.Add(cat);
        Db.SkillItems.Add(item);
        var skill = UserSkill.Create(_talentId, item.Id, 4, null);
        skill.Approve(_managerId);
        Db.UserSkills.Add(skill);

        // Must: 今期の評価目標
        var period = CreateOpenGoalPeriod();
        Db.EvaluationPeriods.Add(period);
        var goal = Goal.Create(_talentId, period.Id, WillCanMust.Must, "今期目標", null, 1);
        Db.Goals.Add(goal);
        await Db.SaveChangesAsync();

        var result = await _svc.GetIntegratedViewAsync(_talentId, _talentId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.Will);
        Assert.Equal("長期目標", result.Value.Will!.LongTermGoal);
        Assert.Single(result.Value.Can);
        Assert.Equal("C#", result.Value.Can[0].SkillItemName);
        Assert.Single(result.Value.Must);
        Assert.Equal("今期目標", result.Value.Must[0].Title);
    }

    [Fact]
    public async Task AC02_02_今期目標未登録の場合MustはEmpty()
    {
        await _svc.CreateAsync(_talentId, _talentId, new SaveCareerPlanRequest("長期目標", "中期目標", "注力"));

        var result = await _svc.GetIntegratedViewAsync(_talentId, _talentId);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Must);
    }

    [Fact]
    public async Task AC02_03_承認済みスキルが0件の場合CanはEmpty()
    {
        await _svc.CreateAsync(_talentId, _talentId, new SaveCareerPlanRequest("長期目標", "中期目標", "注力"));

        var result = await _svc.GetIntegratedViewAsync(_talentId, _talentId);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Can);
    }

    [Fact]
    public async Task AC02_04_直属上長が部下の統合ビューを閲覧できる()
    {
        await _svc.CreateAsync(_talentId, _talentId, new SaveCareerPlanRequest("長期目標", "中期目標", "注力"));

        var result = await _svc.GetIntegratedViewAsync(_managerId, _talentId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.Will);
    }

    [Fact]
    public async Task AC02_04_直属上長でないManagerは部下の統合ビューを閲覧できない_403()
    {
        // otherTalent は managerId の部下ではない
        var result = await _svc.GetIntegratedViewAsync(_managerId, _otherTalentId);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task AC02_04_Senior以上は全タレントの統合ビューを閲覧できる()
    {
        await _svc.CreateAsync(_talentId, _talentId, new SaveCareerPlanRequest("長期目標", "中期目標", "注力"));

        var result = await _svc.GetIntegratedViewAsync(_seniorId, _talentId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.Will);
    }

    // --- REQ-03: 上長コメント・フィードバック ---

    [Fact]
    public async Task AC03_01_直属上長がコメントを投稿できる()
    {
        var created = (await _svc.CreateAsync(_talentId, _talentId, new SaveCareerPlanRequest("目標", "中期", "注力"))).Value!;

        var result = await _svc.AddCommentAsync(_managerId, created.Id, new AddCommentRequest("よく書けています"));

        Assert.True(result.IsSuccess);
        Assert.Equal("よく書けています", result.Value!.Content);
        Assert.Equal("上長花子", result.Value.AuthorName);
    }

    [Fact]
    public async Task AC03_02_タレントが上長コメントを閲覧できる()
    {
        var created = (await _svc.CreateAsync(_talentId, _talentId, new SaveCareerPlanRequest("目標", "中期", "注力"))).Value!;
        await _svc.AddCommentAsync(_managerId, created.Id, new AddCommentRequest("コメント内容"));

        var result = await _svc.GetCommentsAsync(_talentId, created.Id);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("コメント内容", result.Value![0].Content);
    }

    [Fact]
    public async Task AC03_03b_直属上長以外はAPIでコメントできない_403()
    {
        var created = (await _svc.CreateAsync(_talentId, _talentId, new SaveCareerPlanRequest("目標", "中期", "注力"))).Value!;

        // senior は talent の直属上長ではない
        var result = await _svc.AddCommentAsync(_seniorId, created.Id, new AddCommentRequest("不正コメント"));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task AC03_04_空コメントは投稿できない_400()
    {
        var created = (await _svc.CreateAsync(_talentId, _talentId, new SaveCareerPlanRequest("目標", "中期", "注力"))).Value!;

        var result = await _svc.AddCommentAsync(_managerId, created.Id, new AddCommentRequest(""));

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task AC03_04_空白のみのコメントは投稿できない_400()
    {
        var created = (await _svc.CreateAsync(_talentId, _talentId, new SaveCareerPlanRequest("目標", "中期", "注力"))).Value!;

        var result = await _svc.AddCommentAsync(_managerId, created.Id, new AddCommentRequest("   "));

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    // --- REQ-04: 履歴管理 ---

    [Fact]
    public async Task AC04_01_新期プラン作成後も旧期プランが保持される()
    {
        var pastYear = DateTime.UtcNow.Year - 1;
        var pastPlan = CareerPlan.Create(_talentId, "過去長期目標", "過去中期目標", "過去注力");
        typeof(CareerPlan).GetProperty("PeriodYear")!.SetValue(pastPlan, pastYear);
        await Db.CareerPlans.AddAsync(pastPlan);
        await Db.SaveChangesAsync();

        await _svc.CreateAsync(_talentId, _talentId, new SaveCareerPlanRequest("今期長期目標", "今期中期目標", "今期注力"));

        var pastResult = await _svc.GetAsync(_talentId, _talentId, pastYear);
        Assert.True(pastResult.IsSuccess);
        Assert.Equal("過去長期目標", pastResult.Value!.LongTermGoal);

        var currentResult = await _svc.GetAsync(_talentId, _talentId, null);
        Assert.True(currentResult.IsSuccess);
        Assert.Equal("今期長期目標", currentResult.Value!.LongTermGoal);
    }

    [Fact]
    public async Task AC04_02_過去の期を選択して参照できる()
    {
        var pastYear = DateTime.UtcNow.Year - 1;
        var pastPlan = CareerPlan.Create(_talentId, "過去目標", "過去中期", "過去注力");
        typeof(CareerPlan).GetProperty("PeriodYear")!.SetValue(pastPlan, pastYear);
        await Db.CareerPlans.AddAsync(pastPlan);
        await _svc.CreateAsync(_talentId, _talentId, new SaveCareerPlanRequest("今期目標", "今期中期", "今期注力"));
        await Db.SaveChangesAsync();

        var result = await _svc.GetAsync(_talentId, _talentId, pastYear);
        Assert.True(result.IsSuccess);
        Assert.Equal(pastYear, result.Value!.PeriodYear);

        var years = await _svc.GetYearsAsync(_talentId, _talentId);
        Assert.True(years.IsSuccess);
        Assert.Contains(pastYear, years.Value!);
        Assert.Contains(DateTime.UtcNow.Year, years.Value!);
    }

    // --- REQ-05: 組織俯瞰ビュー ---

    [Fact]
    public async Task AC05_01_Seniorが全タレントのプランサマリーを取得できる()
    {
        await _svc.CreateAsync(_talentId, _talentId, new SaveCareerPlanRequest("長期目標", "中期目標", "注力テーマ"));

        var result = await _svc.GetOrgOverviewAsync(_seniorId);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count); // talent, otherTalent の2名
        var talentItem = result.Value.FirstOrDefault(i => i.UserId == _talentId);
        Assert.NotNull(talentItem);
        Assert.Equal("注力テーマ", talentItem!.CurrentFocus);
        var otherItem = result.Value.FirstOrDefault(i => i.UserId == _otherTalentId);
        Assert.NotNull(otherItem);
        Assert.Null(otherItem!.CurrentFocus); // プラン未登録
    }

    [Fact]
    public async Task AC05_02_TalentとManagerは組織一覧にアクセスできない_403()
    {
        var talentResult = await _svc.GetOrgOverviewAsync(_talentId);
        var managerResult = await _svc.GetOrgOverviewAsync(_managerId);

        Assert.False(talentResult.IsSuccess);
        Assert.Equal(403, talentResult.StatusCode);
        Assert.False(managerResult.IsSuccess);
        Assert.Equal(403, managerResult.StatusCode);
    }
}
