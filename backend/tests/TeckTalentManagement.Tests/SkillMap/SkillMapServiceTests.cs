using NSubstitute;
using TeckTalentManagement.Application.SkillMap;
using TeckTalentManagement.Domain.Entities;
using TeckTalentManagement.Domain.Enums;

namespace TeckTalentManagement.Tests.SkillMap;

public class SkillMapServiceTests
{
    private readonly ISkillMapRepository _repo = Substitute.For<ISkillMapRepository>();
    private readonly SkillMapService _sut;

    public SkillMapServiceTests() => _sut = new SkillMapService(_repo);

    private static readonly Guid SeniorId = Guid.NewGuid();
    private static readonly Guid TalentId = Guid.NewGuid();

    private static User NewUser(UserRole role) =>
        User.Create("oid", "テストユーザー", "test@example.com", role);

    private static SkillCategory NewCategory() => SkillCategory.Create("言語", 1);

    private static SkillItem NewSkillItem(Guid categoryId) => SkillItem.Create(categoryId, "C#", 1);

    // --- GetSkillMap ---

    [Fact]
    public async Task GetSkillMap_WhenUserNotFound_ReturnsForbidden()
    {
        _repo.GetUserRoleAsync(SeniorId, Arg.Any<CancellationToken>()).Returns((UserRole?)null);

        var result = await _sut.GetSkillMapAsync(SeniorId);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Theory]
    [InlineData(UserRole.Talent)]
    [InlineData(UserRole.Manager)]
    public async Task GetSkillMap_WhenInsufficientRole_ReturnsForbidden(UserRole role)
    {
        _repo.GetUserRoleAsync(SeniorId, Arg.Any<CancellationToken>()).Returns(role);

        var result = await _sut.GetSkillMapAsync(SeniorId);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Theory]
    [InlineData(UserRole.Senior)]
    [InlineData(UserRole.Executive)]
    [InlineData(UserRole.Admin)]
    public async Task GetSkillMap_WhenAuthorized_ReturnsMap(UserRole role)
    {
        _repo.GetUserRoleAsync(SeniorId, Arg.Any<CancellationToken>()).Returns(role);
        _repo.GetTalentUsersAsync(Arg.Any<CancellationToken>()).Returns(new List<User>());
        _repo.GetApprovedSkillsForTalentsAsync(Arg.Any<CancellationToken>()).Returns(new List<UserSkill>());

        var result = await _sut.GetSkillMapAsync(SeniorId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task GetSkillMap_ReturnsOnlyApprovedSkillsGroupedByCategory()
    {
        var category = NewCategory();
        var item = NewSkillItem(category.Id);

        var skill = UserSkill.Create(TalentId, item.Id, 3, null);
        SetSkillItem(skill, item, category);

        _repo.GetUserRoleAsync(SeniorId, Arg.Any<CancellationToken>()).Returns(UserRole.Senior);
        _repo.GetTalentUsersAsync(Arg.Any<CancellationToken>())
            .Returns(new List<User> { NewUser(UserRole.Talent) });
        _repo.GetApprovedSkillsForTalentsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<UserSkill> { skill });

        var result = await _sut.GetSkillMapAsync(SeniorId);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Categories);
        Assert.Equal(category.Name, result.Value.Categories[0].CategoryName);
        Assert.Single(result.Value.Categories[0].Items);
        Assert.Equal(item.Name, result.Value.Categories[0].Items[0].SkillItemName);
        Assert.Single(result.Value.Categories[0].Items[0].UserLevels);
        Assert.Equal(3, result.Value.Categories[0].Items[0].UserLevels[0].Level);
    }

    private static void SetSkillItem(UserSkill skill, SkillItem item, SkillCategory category)
    {
        // リフレクションでナビゲーションプロパティをセット
        var itemProp = typeof(UserSkill).GetProperty("SkillItem")!;
        itemProp.SetValue(skill, item);

        var catProp = typeof(SkillItem).GetProperty("Category")!;
        catProp.SetValue(item, category);
    }
}
