using SAIMRS.Domain.Entities;

namespace SAIMRS.Application.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> ListAsync(CancellationToken ct = default);

    Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct = default);

    /// <summary>Category id → number of items (active and archived).</summary>
    Task<Dictionary<int, int>> GetItemCountsAsync(CancellationToken ct = default);

    Task<bool> HasItemsAsync(int categoryId, CancellationToken ct = default);

    Task AddAsync(Category category, CancellationToken ct = default);

    void Remove(Category category);
}
