using SAIMRS.Application.Common;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Common;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Application.Features.Categories;

internal sealed class CategoryService(
    ICategoryRepository categories,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICategoryService
{
    public async Task<List<CategoryDto>> ListAsync(CancellationToken ct = default)
    {
        var list = await categories.ListAsync(ct);
        var counts = await categories.GetItemCountsAsync(ct);
        return list
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, counts.GetValueOrDefault(c.Id), c.CreatedAt))
            .ToList();
    }

    public async Task<Result> SaveAsync(SaveCategoryRequest request, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(request.Name) &&
            await categories.NameExistsAsync(request.Name.Trim(), request.Id, ct))
            return Result.Failure($"A category named \"{request.Name.Trim()}\" already exists.");

        try
        {
            if (request.Id is { } id)
            {
                var category = await categories.GetByIdAsync(id, ct);
                if (category is null)
                    return Result.Failure("Category not found.");
                category.Update(request.Name, request.Description, clock.Now);
            }
            else
            {
                await categories.AddAsync(new Category(request.Name, request.Description, clock.Now), ct);
            }

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await categories.GetByIdAsync(id, ct);
        if (category is null)
            return Result.Failure("Category not found.");

        if (await categories.HasItemsAsync(id, ct))
            return Result.Failure(
                $"\"{category.Name}\" still has items (including archived ones). Move or remove those items first.");

        categories.Remove(category);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
