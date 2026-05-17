using TeckTalentManagement.Application.Common;
using TeckTalentManagement.Domain.Entities;

namespace TeckTalentManagement.Application.SkillMaster;

public interface ISkillMasterService
{
    Task<Result<IReadOnlyList<SkillCategoryDto>>> GetCategoriesAsync(CancellationToken ct = default);
    Task<Result<SkillCategoryDto>> CreateCategoryAsync(Guid requesterId, CreateSkillCategoryRequest req, CancellationToken ct = default);
    Task<Result<SkillCategoryDto>> UpdateCategoryAsync(Guid requesterId, Guid categoryId, UpdateSkillCategoryRequest req, CancellationToken ct = default);
    Task<Result<bool>> DeleteCategoryAsync(Guid requesterId, Guid categoryId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<SkillItemDto>>> GetItemsAsync(Guid categoryId, CancellationToken ct = default);
    Task<Result<SkillItemDto>> CreateItemAsync(Guid requesterId, Guid categoryId, CreateSkillItemRequest req, CancellationToken ct = default);
    Task<Result<SkillItemDto>> UpdateItemAsync(Guid requesterId, Guid itemId, UpdateSkillItemRequest req, CancellationToken ct = default);
    Task<Result<bool>> DeleteItemAsync(Guid requesterId, Guid itemId, CancellationToken ct = default);
}

public interface ISkillMasterRepository
{
    Task<bool> IsAdminAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<SkillCategory>> GetAllCategoriesAsync(CancellationToken ct);
    Task<SkillCategory?> FindCategoryAsync(Guid id, CancellationToken ct);
    Task AddCategoryAsync(SkillCategory category, CancellationToken ct);
    Task DeleteCategoryAsync(SkillCategory category, CancellationToken ct);
    Task<bool> HasItemsAsync(Guid categoryId, CancellationToken ct);
    Task<IReadOnlyList<SkillItem>> GetItemsByCategoryAsync(Guid categoryId, CancellationToken ct);
    Task<SkillItem?> FindItemAsync(Guid id, CancellationToken ct);
    Task AddItemAsync(SkillItem item, CancellationToken ct);
    Task DeleteItemAsync(SkillItem item, CancellationToken ct);
    Task<bool> IsItemInUseAsync(Guid itemId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public class SkillMasterService(ISkillMasterRepository repo) : ISkillMasterService
{
    public async Task<Result<IReadOnlyList<SkillCategoryDto>>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var categories = await repo.GetAllCategoriesAsync(ct);
        return Result<IReadOnlyList<SkillCategoryDto>>.Ok(categories.Select(ToCategoryDto).ToList());
    }

    public async Task<Result<SkillCategoryDto>> CreateCategoryAsync(Guid requesterId, CreateSkillCategoryRequest req, CancellationToken ct = default)
    {
        if (!await repo.IsAdminAsync(requesterId, ct))
            return Result<SkillCategoryDto>.Forbidden("管理者のみ操作できます。");

        var category = SkillCategory.Create(req.Name, req.DisplayOrder);
        await repo.AddCategoryAsync(category, ct);
        await repo.SaveChangesAsync(ct);

        return Result<SkillCategoryDto>.Ok(ToCategoryDto(category));
    }

    public async Task<Result<SkillCategoryDto>> UpdateCategoryAsync(Guid requesterId, Guid categoryId, UpdateSkillCategoryRequest req, CancellationToken ct = default)
    {
        if (!await repo.IsAdminAsync(requesterId, ct))
            return Result<SkillCategoryDto>.Forbidden("管理者のみ操作できます。");

        var category = await repo.FindCategoryAsync(categoryId, ct);
        if (category is null)
            return Result<SkillCategoryDto>.NotFound("スキルカテゴリが見つかりません。");

        category.Update(req.Name, req.DisplayOrder);
        await repo.SaveChangesAsync(ct);

        return Result<SkillCategoryDto>.Ok(ToCategoryDto(category));
    }

    public async Task<Result<bool>> DeleteCategoryAsync(Guid requesterId, Guid categoryId, CancellationToken ct = default)
    {
        if (!await repo.IsAdminAsync(requesterId, ct))
            return Result<bool>.Forbidden("管理者のみ操作できます。");

        var category = await repo.FindCategoryAsync(categoryId, ct);
        if (category is null)
            return Result<bool>.NotFound("スキルカテゴリが見つかりません。");

        if (await repo.HasItemsAsync(categoryId, ct))
            return Result<bool>.Conflict("このカテゴリにはスキルが登録されています。先にスキル種別を削除してください。");

        await repo.DeleteCategoryAsync(category, ct);
        await repo.SaveChangesAsync(ct);

        return Result<bool>.Ok(true);
    }

    public async Task<Result<IReadOnlyList<SkillItemDto>>> GetItemsAsync(Guid categoryId, CancellationToken ct = default)
    {
        var category = await repo.FindCategoryAsync(categoryId, ct);
        if (category is null)
            return Result<IReadOnlyList<SkillItemDto>>.NotFound("スキルカテゴリが見つかりません。");

        var items = await repo.GetItemsByCategoryAsync(categoryId, ct);
        return Result<IReadOnlyList<SkillItemDto>>.Ok(items.Select(ToItemDto).ToList());
    }

    public async Task<Result<SkillItemDto>> CreateItemAsync(Guid requesterId, Guid categoryId, CreateSkillItemRequest req, CancellationToken ct = default)
    {
        if (!await repo.IsAdminAsync(requesterId, ct))
            return Result<SkillItemDto>.Forbidden("管理者のみ操作できます。");

        var category = await repo.FindCategoryAsync(categoryId, ct);
        if (category is null)
            return Result<SkillItemDto>.NotFound("スキルカテゴリが見つかりません。");

        var item = SkillItem.Create(categoryId, req.Name, req.DisplayOrder);
        await repo.AddItemAsync(item, ct);
        await repo.SaveChangesAsync(ct);

        return Result<SkillItemDto>.Ok(ToItemDto(item));
    }

    public async Task<Result<SkillItemDto>> UpdateItemAsync(Guid requesterId, Guid itemId, UpdateSkillItemRequest req, CancellationToken ct = default)
    {
        if (!await repo.IsAdminAsync(requesterId, ct))
            return Result<SkillItemDto>.Forbidden("管理者のみ操作できます。");

        var item = await repo.FindItemAsync(itemId, ct);
        if (item is null)
            return Result<SkillItemDto>.NotFound("スキル種別が見つかりません。");

        item.Update(req.Name, req.DisplayOrder);
        await repo.SaveChangesAsync(ct);

        return Result<SkillItemDto>.Ok(ToItemDto(item));
    }

    public async Task<Result<bool>> DeleteItemAsync(Guid requesterId, Guid itemId, CancellationToken ct = default)
    {
        if (!await repo.IsAdminAsync(requesterId, ct))
            return Result<bool>.Forbidden("管理者のみ操作できます。");

        var item = await repo.FindItemAsync(itemId, ct);
        if (item is null)
            return Result<bool>.NotFound("スキル種別が見つかりません。");

        if (await repo.IsItemInUseAsync(itemId, ct))
            return Result<bool>.Conflict("使用中のスキルがあります。タレントのスキルから削除してから再度お試しください。");

        await repo.DeleteItemAsync(item, ct);
        await repo.SaveChangesAsync(ct);

        return Result<bool>.Ok(true);
    }

    private static SkillCategoryDto ToCategoryDto(SkillCategory c) =>
        new(c.Id, c.Name, c.DisplayOrder, c.CreatedAt, c.UpdatedAt);

    private static SkillItemDto ToItemDto(SkillItem i) =>
        new(i.Id, i.CategoryId, i.Name, i.DisplayOrder, i.CreatedAt, i.UpdatedAt);
}
