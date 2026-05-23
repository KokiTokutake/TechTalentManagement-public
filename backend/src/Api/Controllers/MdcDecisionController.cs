using Microsoft.AspNetCore.Mvc;
using TeckTalentManagement.Application.MdcDecision;
using TeckTalentManagement.Domain.Enums;

namespace TeckTalentManagement.Api.Controllers;

[ApiController]
public class MdcDecisionController(IMdcDecisionService service) : ControllerBase
{
    // GET /api/mdc-sessions/{sessionId}/decisions
    [HttpGet("api/mdc-sessions/{sessionId:guid}/decisions")]
    public async Task<IActionResult> GetDecisions(Guid sessionId, CancellationToken ct)
    {
        var result = await service.GetDecisionsAsync(GetCurrentUserId(), sessionId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // POST /api/mdc-sessions/{sessionId}/decisions
    [HttpPost("api/mdc-sessions/{sessionId:guid}/decisions")]
    public async Task<IActionResult> CreateDecision(Guid sessionId, [FromBody] CreateDecisionRequest req, CancellationToken ct)
    {
        var result = await service.CreateDecisionAsync(GetCurrentUserId(), sessionId, req, ct);
        return result.IsSuccess ? StatusCode(201, result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // PUT /api/mdc-decisions/{id}
    [HttpPut("api/mdc-decisions/{id:guid}")]
    public async Task<IActionResult> UpdateDecision(Guid id, [FromBody] UpdateDecisionRequest req, CancellationToken ct)
    {
        var result = await service.UpdateDecisionAsync(GetCurrentUserId(), id, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // POST /api/mdc-decisions/{decisionId}/action-items
    [HttpPost("api/mdc-decisions/{decisionId:guid}/action-items")]
    public async Task<IActionResult> AddActionItem(Guid decisionId, [FromBody] CreateActionItemRequest req, CancellationToken ct)
    {
        var result = await service.AddActionItemAsync(GetCurrentUserId(), decisionId, req, ct);
        return result.IsSuccess ? StatusCode(201, result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // GET /api/mdc-sessions/{sessionId}/action-items
    [HttpGet("api/mdc-sessions/{sessionId:guid}/action-items")]
    public async Task<IActionResult> GetActionItems(
        Guid sessionId,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        MdcActionItemStatus? parsedStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<MdcActionItemStatus>(status, out var s))
            parsedStatus = s;

        var result = await service.GetActionItemsAsync(GetCurrentUserId(), sessionId, parsedStatus, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // PATCH /api/mdc-action-items/{id}/status
    [HttpPatch("api/mdc-action-items/{id:guid}/status")]
    public async Task<IActionResult> UpdateActionItemStatus(Guid id, [FromBody] UpdateActionItemStatusRequest req, CancellationToken ct)
    {
        var result = await service.UpdateActionItemStatusAsync(GetCurrentUserId(), id, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // GET /api/talents/{talentId}/mdc-decisions
    [HttpGet("api/talents/{talentId:guid}/mdc-decisions")]
    public async Task<IActionResult> GetDecisionsByTalent(Guid talentId, CancellationToken ct)
    {
        var result = await service.GetDecisionsByTalentAsync(GetCurrentUserId(), talentId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // GET /api/users/{userId}/action-items
    [HttpGet("api/users/{userId:guid}/action-items")]
    public async Task<IActionResult> GetActionItemsByUser(
        Guid userId,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        bool pendingOnly = status == "pending";
        var result = await service.GetActionItemsByUserAsync(GetCurrentUserId(), userId, pendingOnly, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    private Guid GetCurrentUserId()
    {
        var header = Request.Headers["X-User-Id"].FirstOrDefault();
        return Guid.TryParse(header, out var id) ? id : Guid.Empty;
    }
}
