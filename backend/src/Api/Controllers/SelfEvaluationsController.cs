using Microsoft.AspNetCore.Mvc;
using TeckTalentManagement.Application.SelfEvaluations;

namespace TeckTalentManagement.Api.Controllers;

[ApiController]
[Route("api/periods/{periodId:guid}/users/{userId:guid}/self-evaluation")]
public class SelfEvaluationsController(ISelfEvaluationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid periodId, Guid userId, CancellationToken ct)
    {
        var result = await service.GetAsync(userId, periodId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPut("draft")]
    public async Task<IActionResult> SaveDraft(
        Guid periodId, Guid userId,
        [FromBody] SaveDraftRequest req,
        CancellationToken ct)
    {
        var result = await service.SaveDraftAsync(userId, periodId, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit(Guid periodId, Guid userId, CancellationToken ct)
    {
        var result = await service.SubmitAsync(userId, periodId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, result.Error);
    }
}
