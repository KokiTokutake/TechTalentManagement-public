using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.Notification;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController(INotificationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromHeader(Name = "X-User-Id")] Guid requesterId,
        [FromQuery] Guid userId,
        [FromQuery] bool unreadOnly = false,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { error = "userId は必須です。" });

        var result = await service.GetListAsync(requesterId, userId, unreadOnly, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(
        [FromHeader(Name = "X-User-Id")] Guid requesterId,
        [FromQuery] Guid userId,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { error = "userId は必須です。" });

        var result = await service.GetUnreadCountAsync(requesterId, userId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(
        Guid id,
        [FromHeader(Name = "X-User-Id")] Guid requesterId,
        CancellationToken ct = default)
    {
        var result = await service.MarkAsReadAsync(requesterId, id, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(
        [FromHeader(Name = "X-User-Id")] Guid requesterId,
        [FromQuery] Guid userId,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return BadRequest(new { error = "userId は必須です。" });

        var result = await service.MarkAllAsReadAsync(requesterId, userId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
