using NSubstitute;
using TechTalentManagement.Application.SkillMaster;
using TechTalentManagement.Domain.Entities;

namespace TechTalentManagement.Tests.SkillMaster;

public class SkillMasterServiceTests
{
    private readonly ISkillMasterRepository _repo = Substitute.For<ISkillMasterRepository>();
    private readonly SkillMasterService _sut;

    public SkillMasterServiceTests() => _sut = new SkillMasterService(_repo);

    private static readonly Guid AdminId = Guid.NewGuid();
    private static readonly Guid NonAdminId = Guid.NewGuid();

    private static SkillCategory NewCategory(string name = "言語") =>
        SkillCategory.Create(name, 1);

    private static SkillItem NewItem(Guid categoryId, string name = "C#") =>
        SkillItem.Create(categoryId, name, 1);

    // --- CreateCategory ---

    [Fact]
    public async Task CreateCategory_WhenNotAdmin_ReturnsForbidden()
    {
        _repo.IsAdminAsync(NonAdminId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.CreateCategoryAsync(NonAdminId, new CreateSkillCategoryRequest("言語", 1));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WhenAdmin_ReturnsCreatedDto()
    {
        _repo.IsAdminAsync(AdminId, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.CreateCategoryAsync(AdminId, new CreateSkillCategoryRequest("言語", 1));

        Assert.True(result.IsSuccess);
        Assert.Equal("言語", result.Value!.Name);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // --- DeleteCategory ---

    [Fact]
    public async Task DeleteCategory_WhenCategoryNotFound_ReturnsNotFound()
    {
        _repo.IsAdminAsync(AdminId, Arg.Any<CancellationToken>()).Returns(true);
        _repo.FindCategoryAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SkillCategory?)null);

        var result = await _sut.DeleteCategoryAsync(AdminId, Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_WhenHasItems_ReturnsConflict()
    {
        var category = NewCategory();
        _repo.IsAdminAsync(AdminId, Arg.Any<CancellationToken>()).Returns(true);
        _repo.FindCategoryAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        _repo.HasItemsAsync(category.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.DeleteCategoryAsync(AdminId, category.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.StatusCode);
    }

    // --- DeleteItem ---

    [Fact]
    public async Task DeleteItem_WhenInUse_ReturnsConflict()
    {
        var category = NewCategory();
        var item = NewItem(category.Id);
        _repo.IsAdminAsync(AdminId, Arg.Any<CancellationToken>()).Returns(true);
        _repo.FindItemAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        _repo.IsItemInUseAsync(item.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.DeleteItemAsync(AdminId, item.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task DeleteItem_WhenNotInUse_Succeeds()
    {
        var category = NewCategory();
        var item = NewItem(category.Id);
        _repo.IsAdminAsync(AdminId, Arg.Any<CancellationToken>()).Returns(true);
        _repo.FindItemAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        _repo.IsItemInUseAsync(item.Id, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.DeleteItemAsync(AdminId, item.Id);

        Assert.True(result.IsSuccess);
        await _repo.Received(1).DeleteItemAsync(item, Arg.Any<CancellationToken>());
    }

    // --- GetItems ---

    [Fact]
    public async Task GetItems_WhenCategoryNotFound_ReturnsNotFound()
    {
        _repo.FindCategoryAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SkillCategory?)null);

        var result = await _sut.GetItemsAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task GetItems_WhenCategoryExists_ReturnsItems()
    {
        var category = NewCategory();
        var items = new List<SkillItem> { NewItem(category.Id), NewItem(category.Id, "Java") };
        _repo.FindCategoryAsync(category.Id, Arg.Any<CancellationToken>()).Returns(category);
        _repo.GetItemsByCategoryAsync(category.Id, Arg.Any<CancellationToken>()).Returns(items);

        var result = await _sut.GetItemsAsync(category.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }
}
