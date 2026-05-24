using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.FeedbackMeeting;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/feedback-meetings")]
public class FeedbackMeetingsController(IFeedbackMeetingService service) : ControllerBase
{
    private Guid RequesterId => Guid.TryParse(Request.Headers["X-User-Id"], out var id) ? id : Guid.Empty;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeedbackMeetingRequest req, CancellationToken ct)
    {
        var result = await service.CreateAsync(RequesterId, req, ct);
        return result.IsSuccess
            ? StatusCode(201, result.Value)
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetByTalentAndPeriod([FromQuery] Guid talentId, [FromQuery] Guid periodId, CancellationToken ct)
    {
        var result = await service.GetByTalentAndPeriodAsync(RequesterId, talentId, periodId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPatch("{id:guid}/disclose")]
    public async Task<IActionResult> Disclose(Guid id, CancellationToken ct)
    {
        var result = await service.DiscloseAsync(RequesterId, id, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
