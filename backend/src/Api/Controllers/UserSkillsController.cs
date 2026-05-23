using Microsoft.AspNetCore.Mvc;
using TechTalentManagement.Application.UserSkills;

namespace TechTalentManagement.Api.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/skills")]
public class UserSkillsController(IUserSkillService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUserSkills(Guid userId, CancellationToken ct)
    {
        var result = await service.GetUserSkillsAsync(userId, ct);
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> AddSkill(Guid userId, [FromQuery] Guid requesterId, [FromBody] AddUserSkillRequest req, CancellationToken ct)
    {
        var result = await service.AddSkillAsync(requesterId, userId, req, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return CreatedAtAction(nameof(GetUserSkills), new { userId }, result.Value);
    }
}

[ApiController]
[Route("api/user-skills")]
public class UserSkillItemController(IUserSkillService service) : ControllerBase
{
    [HttpPut("{userSkillId:guid}")]
    public async Task<IActionResult> UpdateSkill(Guid userSkillId, [FromQuery] Guid requesterId, [FromBody] UpdateUserSkillRequest req, CancellationToken ct)
    {
        var result = await service.UpdateSkillAsync(requesterId, userSkillId, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpDelete("{userSkillId:guid}")]
    public async Task<IActionResult> DeleteSkill(Guid userSkillId, [FromQuery] Guid requesterId, CancellationToken ct)
    {
        var result = await service.DeleteSkillAsync(requesterId, userSkillId, ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("{userSkillId:guid}/approve")]
    public async Task<IActionResult> ApproveSkill(Guid userSkillId, [FromQuery] Guid requesterId, CancellationToken ct)
    {
        var result = await service.ApproveSkillAsync(requesterId, userSkillId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("{userSkillId:guid}/reject")]
    public async Task<IActionResult> RejectSkill(Guid userSkillId, [FromQuery] Guid requesterId, [FromBody] RejectSkillRequest req, CancellationToken ct)
    {
        var result = await service.RejectSkillAsync(requesterId, userSkillId, req.Reason, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
