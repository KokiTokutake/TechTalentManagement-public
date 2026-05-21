using Microsoft.AspNetCore.Mvc;
using TeckTalentManagement.Application.OneOnOne;

namespace TeckTalentManagement.Api.Controllers;

[ApiController]
public class OneOnOnesController(IOneOnOneService service) : ControllerBase
{
    // GET /api/one-on-ones?talentId=&page=&limit=
    [HttpGet("api/one-on-ones")]
    public async Task<IActionResult> GetMeetings(
        [FromQuery] Guid talentId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        var result = await service.GetMeetingsAsync(talentId, page, limit, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // POST /api/one-on-ones
    [HttpPost("api/one-on-ones")]
    public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingRequest req, CancellationToken ct = default)
    {
        // TODO: T-002 認証実装後に HttpContext から managerId を取得する
        var managerId = GetCurrentUserId();
        var result = await service.CreateMeetingAsync(managerId, req, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return CreatedAtAction(nameof(GetMeeting), new { id = result.Value!.Id }, result.Value);
    }

    // GET /api/one-on-ones/{id}
    [HttpGet("api/one-on-ones/{id:guid}")]
    public async Task<IActionResult> GetMeeting(Guid id, CancellationToken ct = default)
    {
        var requesterId = GetCurrentUserId();
        var result = await service.GetMeetingAsync(id, requesterId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // PUT /api/one-on-ones/{id}
    [HttpPut("api/one-on-ones/{id:guid}")]
    public async Task<IActionResult> UpdateMeeting(Guid id, [FromBody] UpdateMeetingRequest req, CancellationToken ct = default)
    {
        var managerId = GetCurrentUserId();
        var result = await service.UpdateMeetingAsync(id, managerId, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // DELETE /api/one-on-ones/{id}
    [HttpDelete("api/one-on-ones/{id:guid}")]
    public async Task<IActionResult> DeleteMeeting(Guid id, CancellationToken ct = default)
    {
        var managerId = GetCurrentUserId();
        var result = await service.DeleteMeetingAsync(id, managerId, ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // GET /api/one-on-ones/{id}/action-items
    [HttpGet("api/one-on-ones/{id:guid}/action-items")]
    public async Task<IActionResult> GetActionItems(Guid id, CancellationToken ct = default)
    {
        var result = await service.GetActionItemsAsync(id, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // POST /api/one-on-ones/{id}/action-items
    [HttpPost("api/one-on-ones/{id:guid}/action-items")]
    public async Task<IActionResult> AddActionItem(Guid id, [FromBody] AddActionItemRequest req, CancellationToken ct = default)
    {
        var managerId = GetCurrentUserId();
        var result = await service.AddActionItemAsync(id, managerId, req, ct);
        return result.IsSuccess ? StatusCode(201, result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // PATCH /api/action-items/{itemId}/status
    [HttpPatch("api/action-items/{itemId:guid}/status")]
    public async Task<IActionResult> UpdateActionItemStatus(Guid itemId, [FromBody] UpdateActionItemStatusRequest req, CancellationToken ct = default)
    {
        var managerId = GetCurrentUserId();
        var result = await service.UpdateActionItemStatusAsync(itemId, managerId, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // GET /api/talents/{talentId}/one-on-one-summary
    [HttpGet("api/talents/{talentId:guid}/one-on-one-summary")]
    public async Task<IActionResult> GetOneOnOneSummary(Guid talentId, CancellationToken ct = default)
    {
        var result = await service.GetOneOnOneSummaryAsync(talentId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // GET /api/talents/{talentId}/previous-meeting
    [HttpGet("api/talents/{talentId:guid}/previous-meeting")]
    public async Task<IActionResult> GetPreviousMeeting(Guid talentId, CancellationToken ct = default)
    {
        var result = await service.GetPreviousMeetingAsync(talentId, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return result.Value is null ? NoContent() : Ok(result.Value);
    }

    // TODO: T-002 認証実装後に ClaimsPrincipal から取得する
    // 開発中は X-User-Id ヘッダーで上書き可能
    private Guid GetCurrentUserId()
    {
        var header = HttpContext.Request.Headers["X-User-Id"].FirstOrDefault();
        if (header is not null && Guid.TryParse(header, out var id))
            return id;

        return Guid.Parse("00000000-0000-0000-0000-000000000002");
    }
}
