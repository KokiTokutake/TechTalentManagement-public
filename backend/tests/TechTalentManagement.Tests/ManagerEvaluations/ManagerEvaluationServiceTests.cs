using NSubstitute;
using TechTalentManagement.Application.ManagerEvaluations;
using TechTalentManagement.Application.Notification;
using TechTalentManagement.Domain.Entities;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Tests.ManagerEvaluations;

public class ManagerEvaluationServiceTests
{
    private readonly IManagerEvaluationRepository _repo = Substitute.For<IManagerEvaluationRepository>();
    private readonly INotificationService _notificationService = Substitute.For<INotificationService>();
    private readonly ManagerEvaluationService _sut;

    public ManagerEvaluationServiceTests() => _sut = new ManagerEvaluationService(_repo, _notificationService);

    private static SelfEvaluation SubmittedSelfEvaluation(Guid userId, Guid periodId)
    {
        var eval = SelfEvaluation.Create(userId, periodId);
        eval.Submit();
        return eval;
    }

    [Fact]
    public async Task SaveDraft_WhenSelfEvaluationNotFound_ReturnsNotFound()
    {
        _repo.FindSelfEvaluationAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
             .Returns((SelfEvaluation?)null);

        var result = await _sut.SaveDraftAsync(Guid.NewGuid(), Guid.NewGuid(),
            new SaveManagerDraftRequest(3, "コメント"));

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task SaveDraft_WhenSelfEvaluationNotSubmitted_ReturnsForbidden()
    {
        var selfEval = SelfEvaluation.Create(Guid.NewGuid(), Guid.NewGuid()); // 未提出
        _repo.FindSelfEvaluationAsync(selfEval.Id, Arg.Any<CancellationToken>()).Returns(selfEval);

        var result = await _sut.SaveDraftAsync(Guid.NewGuid(), selfEval.Id,
            new SaveManagerDraftRequest(3, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task SaveDraft_WhenAlreadyConfirmed_ReturnsConflict()
    {
        var managerId = Guid.NewGuid();
        var selfEval = SubmittedSelfEvaluation(Guid.NewGuid(), Guid.NewGuid());
        var managerEval = ManagerEvaluation.Create(selfEval.Id, managerId);
        managerEval.UpdateDraft(4, "良い");
        managerEval.Confirm();

        _repo.FindSelfEvaluationAsync(selfEval.Id, Arg.Any<CancellationToken>()).Returns(selfEval);
        _repo.FindAsync(selfEval.Id, Arg.Any<CancellationToken>()).Returns(managerEval);

        var result = await _sut.SaveDraftAsync(managerId, selfEval.Id,
            new SaveManagerDraftRequest(3, "変更"));

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task SaveDraft_WhenOtherManager_ReturnsForbidden()
    {
        var originalManagerId = Guid.NewGuid();
        var otherManagerId = Guid.NewGuid();
        var selfEval = SubmittedSelfEvaluation(Guid.NewGuid(), Guid.NewGuid());
        var managerEval = ManagerEvaluation.Create(selfEval.Id, originalManagerId);

        _repo.FindSelfEvaluationAsync(selfEval.Id, Arg.Any<CancellationToken>()).Returns(selfEval);
        _repo.FindAsync(selfEval.Id, Arg.Any<CancellationToken>()).Returns(managerEval);

        var result = await _sut.SaveDraftAsync(otherManagerId, selfEval.Id,
            new SaveManagerDraftRequest(3, null));

        Assert.False(result.IsSuccess);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public async Task SaveDraft_WhenValid_CreatesAndReturnsDraft()
    {
        var managerId = Guid.NewGuid();
        var selfEval = SubmittedSelfEvaluation(Guid.NewGuid(), Guid.NewGuid());

        _repo.FindSelfEvaluationAsync(selfEval.Id, Arg.Any<CancellationToken>()).Returns(selfEval);
        _repo.FindAsync(selfEval.Id, Arg.Any<CancellationToken>()).Returns((ManagerEvaluation?)null);

        var result = await _sut.SaveDraftAsync(managerId, selfEval.Id,
            new SaveManagerDraftRequest(4, "優秀です"));

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value!.OverallScore);
        Assert.Equal("優秀です", result.Value.FeedbackComment);
        Assert.False(result.Value.IsConfirmed);
        await _repo.Received(1).AddAsync(Arg.Any<ManagerEvaluation>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Confirm_WhenScoreIsZero_ReturnsBadRequest()
    {
        var managerId = Guid.NewGuid();
        var selfEval = SubmittedSelfEvaluation(Guid.NewGuid(), Guid.NewGuid());
        var managerEval = ManagerEvaluation.Create(selfEval.Id, managerId); // score = 0

        _repo.FindSelfEvaluationAsync(selfEval.Id, Arg.Any<CancellationToken>()).Returns(selfEval);
        _repo.FindAsync(selfEval.Id, Arg.Any<CancellationToken>()).Returns(managerEval);

        var result = await _sut.ConfirmAsync(managerId, selfEval.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task Confirm_WhenAlreadyConfirmed_ReturnsConflict()
    {
        var managerId = Guid.NewGuid();
        var selfEval = SubmittedSelfEvaluation(Guid.NewGuid(), Guid.NewGuid());
        var managerEval = ManagerEvaluation.Create(selfEval.Id, managerId);
        managerEval.UpdateDraft(5, null);
        managerEval.Confirm();

        _repo.FindSelfEvaluationAsync(selfEval.Id, Arg.Any<CancellationToken>()).Returns(selfEval);
        _repo.FindAsync(selfEval.Id, Arg.Any<CancellationToken>()).Returns(managerEval);

        var result = await _sut.ConfirmAsync(managerId, selfEval.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task Confirm_WhenValid_SetsIsConfirmedAndReturnsDto()
    {
        var managerId = Guid.NewGuid();
        var selfEval = SubmittedSelfEvaluation(Guid.NewGuid(), Guid.NewGuid());
        var managerEval = ManagerEvaluation.Create(selfEval.Id, managerId);
        managerEval.UpdateDraft(3, "フィードバック");

        _repo.FindSelfEvaluationAsync(selfEval.Id, Arg.Any<CancellationToken>()).Returns(selfEval);
        _repo.FindAsync(selfEval.Id, Arg.Any<CancellationToken>()).Returns(managerEval);

        var result = await _sut.ConfirmAsync(managerId, selfEval.Id);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsConfirmed);
        Assert.NotNull(result.Value.ConfirmedAt);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
