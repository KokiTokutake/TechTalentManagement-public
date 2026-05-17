using Microsoft.AspNetCore.Mvc;
using TeckTalentManagement.Application.ManagerEvaluations;

namespace TeckTalentManagement.Api.Controllers;

[ApiController]
[Route("api/self-evaluations/{selfEvaluationId:guid}/manager-evaluation")]
public class ManagerEvaluationsController(IManagerEvaluationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        Guid selfEvaluationId,
        [FromQuery] Guid managerId,
        CancellationToken ct)
    {
        var result = await service.GetAsync(managerId, selfEvaluationId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPut("draft")]
    public async Task<IActionResult> SaveDraft(
        Guid selfEvaluationId,
        [FromQuery] Guid managerId,
        [FromBody] SaveManagerDraftRequest req,
        CancellationToken ct)
    {
        var result = await service.SaveDraftAsync(managerId, selfEvaluationId, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm(
        Guid selfEvaluationId,
        [FromQuery] Guid managerId,
        CancellationToken ct)
    {
        var result = await service.ConfirmAsync(managerId, selfEvaluationId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, result.Error);
    }
}
