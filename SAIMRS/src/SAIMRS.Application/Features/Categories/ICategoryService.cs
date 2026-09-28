using SAIMRS.Application.Common;

namespace SAIMRS.Application.Features.Categories;

public interface ICategoryService
{
    Task<List<CategoryDto>> ListAsync(CancellationToken ct = default);

    Task<Result> SaveAsync(SaveCategoryRequest request, CancellationToken ct = default);

    /// <summary>Blocked when the category still has items.</summary>
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
