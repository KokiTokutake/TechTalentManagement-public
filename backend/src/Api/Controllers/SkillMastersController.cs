using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.SkillLevelDefinition;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/skill-masters")]
public class SkillMastersController(ISkillLevelDefinitionService service) : ControllerBase
{
    [HttpGet("{skillItemId:guid}/level-definitions")]
    public async Task<IActionResult> GetLevelDefinitions(Guid skillItemId, CancellationToken ct)
    {
        var result = await service.GetDefinitionsAsync(skillItemId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPut("{skillItemId:guid}/level-definitions")]
    public async Task<IActionResult> SaveLevelDefinitions(
        Guid skillItemId,
        [FromQuery] Guid requesterId,
        [FromBody] SaveSkillLevelDefinitionsRequest req,
        CancellationToken ct)
    {
        var result = await service.SaveDefinitionsAsync(requesterId, skillItemId, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
