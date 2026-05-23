using NSubstitute;
using TechTalentManagement.Application.UserSkills;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Tests.UserSkills;

public class UserSkillServiceTests
{
    private readonly IUserSkillRepository _repo = Substitute.For<IUserSkillRepository>();
    private readonly UserSkillService _sut;

    public UserSkillServiceTests() => _sut = new UserSkillService(_repo);

    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid OtherId = Guid.NewGuid();
    private static readonly Guid SkillItemId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();

    private static SkillCategory NewCategory() => SkillCategory.Create("言語", 1);

    private static SkillItem NewSkillItem()
    {
        var cat = NewCategory();
        return SkillItem.Create(cat.Id, "C#", 1);
    }

    private static UserSkill NewUserSkill(Guid userId, Guid skillItemId)
        => UserSkill.Create(userId, skillItemId, 3, "コメント");

    // --- AddSkill ---

    [Fact]
    public async Task AddSkill_WhenRequesterIsNotOwner_ReturnsForbidden()
    {
        var result = await _sut.AddSkillAsync(OtherId, OwnerId, new AddUserSkillRequest(SkillItemId, 3, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task AddSkill_WhenLevelOutOfRange_ReturnsBadRequest()
    {
        var result = await _sut.AddSkillAsync(OwnerId, OwnerId, new AddUserSkillRequest(SkillItemId, 6, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task AddSkill_WhenSkillItemNotFound_ReturnsNotFound()
    {
        _repo.FindSkillItemAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SkillItem?)null);

        var result = await _sut.AddSkillAsync(OwnerId, OwnerId, new AddUserSkillRequest(SkillItemId, 3, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task AddSkill_WhenAlreadyRegistered_ReturnsConflict()
    {
        var item = NewSkillItem();
        _repo.FindSkillItemAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        _repo.ExistsAsync(OwnerId, item.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.AddSkillAsync(OwnerId, OwnerId, new AddUserSkillRequest(item.Id, 3, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task AddSkill_WhenValid_CreatesWithPendingStatus()
    {
        var item = NewSkillItem();
        var userSkill = NewUserSkill(OwnerId, item.Id);
        _repo.FindSkillItemAsync(item.Id, Arg.Any<CancellationToken>()).Returns(item);
        _repo.ExistsAsync(OwnerId, item.Id, Arg.Any<CancellationToken>()).Returns(false);
        _repo.FindAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(userSkill);

        var result = await _sut.AddSkillAsync(OwnerId, OwnerId, new AddUserSkillRequest(item.Id, 3, "コメント"));

        Assert.True(result.IsSuccess);
        await _repo.Received(1).AddAsync(Arg.Any<UserSkill>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).AddHistoryAsync(Arg.Any<UserSkillHistory>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // --- UpdateSkill ---

    [Fact]
    public async Task UpdateSkill_WhenLevelOutOfRange_ReturnsBadRequest()
    {
        var result = await _sut.UpdateSkillAsync(OwnerId, Guid.NewGuid(), new UpdateUserSkillRequest(0, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task UpdateSkill_WhenNotFound_ReturnsNotFound()
    {
        _repo.FindAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((UserSkill?)null);

        var result = await _sut.UpdateSkillAsync(OwnerId, Guid.NewGuid(), new UpdateUserSkillRequest(4, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task UpdateSkill_WhenRequesterIsNotOwner_ReturnsForbidden()
    {
        var userSkill = NewUserSkill(OwnerId, SkillItemId);
        _repo.FindAsync(userSkill.Id, Arg.Any<CancellationToken>()).Returns(userSkill);

        var result = await _sut.UpdateSkillAsync(OtherId, userSkill.Id, new UpdateUserSkillRequest(4, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task UpdateSkill_WhenValid_ResetsStatusToPending()
    {
        var userSkill = NewUserSkill(OwnerId, SkillItemId);
        _repo.FindAsync(userSkill.Id, Arg.Any<CancellationToken>()).Returns(userSkill);

        var result = await _sut.UpdateSkillAsync(OwnerId, userSkill.Id, new UpdateUserSkillRequest(5, "更新コメント"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Pending", result.Value!.Status);
        await _repo.Received(1).AddHistoryAsync(Arg.Any<UserSkillHistory>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // --- DeleteSkill ---

    [Fact]
    public async Task DeleteSkill_WhenNotFound_ReturnsNotFound()
    {
        _repo.FindAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((UserSkill?)null);

        var result = await _sut.DeleteSkillAsync(OwnerId, Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task DeleteSkill_WhenRequesterIsNotOwner_ReturnsForbidden()
    {
        var userSkill = NewUserSkill(OwnerId, SkillItemId);
        _repo.FindAsync(userSkill.Id, Arg.Any<CancellationToken>()).Returns(userSkill);

        var result = await _sut.DeleteSkillAsync(OtherId, userSkill.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task DeleteSkill_WhenOwner_Succeeds()
    {
        var userSkill = NewUserSkill(OwnerId, SkillItemId);
        _repo.FindAsync(userSkill.Id, Arg.Any<CancellationToken>()).Returns(userSkill);

        var result = await _sut.DeleteSkillAsync(OwnerId, userSkill.Id);

        Assert.True(result.IsSuccess);
        await _repo.Received(1).DeleteAsync(userSkill, Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // --- ApproveSkill ---

    [Fact]
    public async Task ApproveSkill_WhenNotFound_ReturnsNotFound()
    {
        _repo.FindAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((UserSkill?)null);

        var result = await _sut.ApproveSkillAsync(OtherId, Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task ApproveSkill_WhenNotManager_ReturnsForbidden()
    {
        var userSkill = NewUserSkill(OwnerId, SkillItemId);
        _repo.FindAsync(userSkill.Id, Arg.Any<CancellationToken>()).Returns(userSkill);
        _repo.IsManagerOfAsync(OtherId, OwnerId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.ApproveSkillAsync(OtherId, userSkill.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task ApproveSkill_WhenStatusIsNotPending_ReturnsBadRequest()
    {
        var userSkill = NewUserSkill(OwnerId, SkillItemId);
        userSkill.Approve(OtherId);
        _repo.FindAsync(userSkill.Id, Arg.Any<CancellationToken>()).Returns(userSkill);
        _repo.IsManagerOfAsync(OtherId, OwnerId, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.ApproveSkillAsync(OtherId, userSkill.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task ApproveSkill_WhenValid_SetsApprovedStatus()
    {
        var userSkill = NewUserSkill(OwnerId, SkillItemId);
        _repo.FindAsync(userSkill.Id, Arg.Any<CancellationToken>()).Returns(userSkill);
        _repo.IsManagerOfAsync(OtherId, OwnerId, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.ApproveSkillAsync(OtherId, userSkill.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("Approved", result.Value!.Status);
        Assert.Equal(OtherId, result.Value.ApprovedBy);
        await _repo.Received(1).AddHistoryAsync(Arg.Any<UserSkillHistory>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // --- RejectSkill ---

    [Fact]
    public async Task RejectSkill_WhenReasonIsEmpty_ReturnsBadRequest()
    {
        var result = await _sut.RejectSkillAsync(OtherId, Guid.NewGuid(), "");

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task RejectSkill_WhenNotManager_ReturnsForbidden()
    {
        var userSkill = NewUserSkill(OwnerId, SkillItemId);
        _repo.FindAsync(userSkill.Id, Arg.Any<CancellationToken>()).Returns(userSkill);
        _repo.IsManagerOfAsync(OtherId, OwnerId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.RejectSkillAsync(OtherId, userSkill.Id, "理由");

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task RejectSkill_WhenValid_SetsRejectedStatus()
    {
        var userSkill = NewUserSkill(OwnerId, SkillItemId);
        _repo.FindAsync(userSkill.Id, Arg.Any<CancellationToken>()).Returns(userSkill);
        _repo.IsManagerOfAsync(OtherId, OwnerId, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.RejectSkillAsync(OtherId, userSkill.Id, "レベルが実態と合っていない");

        Assert.True(result.IsSuccess);
        Assert.Equal("Rejected", result.Value!.Status);
        Assert.Equal("レベルが実態と合っていない", result.Value.RejectedReason);
        await _repo.Received(1).AddHistoryAsync(Arg.Any<UserSkillHistory>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
