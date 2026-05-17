using Microsoft.AspNetCore.Mvc;
using TeckTalentManagement.Application.SkillMaster;

namespace TeckTalentManagement.Api.Controllers;

[ApiController]
[Route("api/skill-categories")]
public class SkillCategoriesController(ISkillMasterService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var result = await service.GetCategoriesAsync(ct);
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromQuery] Guid requesterId, [FromBody] CreateSkillCategoryRequest req, CancellationToken ct)
    {
        var result = await service.CreateCategoryAsync(requesterId, req, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return CreatedAtAction(nameof(GetCategories), result.Value);
    }

    [HttpPut("{categoryId:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid categoryId, [FromQuery] Guid requesterId, [FromBody] UpdateSkillCategoryRequest req, CancellationToken ct)
    {
        var result = await service.UpdateCategoryAsync(requesterId, categoryId, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpDelete("{categoryId:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid categoryId, [FromQuery] Guid requesterId, CancellationToken ct)
    {
        var result = await service.DeleteCategoryAsync(requesterId, categoryId, ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpGet("{categoryId:guid}/items")]
    public async Task<IActionResult> GetItems(Guid categoryId, CancellationToken ct)
    {
        var result = await service.GetItemsAsync(categoryId, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpPost("{categoryId:guid}/items")]
    public async Task<IActionResult> CreateItem(Guid categoryId, [FromQuery] Guid requesterId, [FromBody] CreateSkillItemRequest req, CancellationToken ct)
    {
        var result = await service.CreateItemAsync(requesterId, categoryId, req, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return CreatedAtAction(nameof(GetItems), new { categoryId }, result.Value);
    }
}

[ApiController]
[Route("api/skill-items")]
public class SkillItemsController(ISkillMasterService service) : ControllerBase
{
    [HttpPut("{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid itemId, [FromQuery] Guid requesterId, [FromBody] UpdateSkillItemRequest req, CancellationToken ct)
    {
        var result = await service.UpdateItemAsync(requesterId, itemId, req, ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    [HttpDelete("{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid itemId, [FromQuery] Guid requesterId, CancellationToken ct)
    {
        var result = await service.DeleteItemAsync(requesterId, itemId, ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
