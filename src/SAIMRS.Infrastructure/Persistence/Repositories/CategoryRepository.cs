using Microsoft.EntityFrameworkCore;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public Task<List<Category>> ListAsync(CancellationToken ct = default) =>
        db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

    public Task<Category?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct = default) =>
        db.Categories.AnyAsync(c => c.Name == name && (excludeId == null || c.Id != excludeId), ct);

    public Task<Dictionary<int, int>> GetItemCountsAsync(CancellationToken ct = default) =>
        db.InventoryItems
            .GroupBy(i => i.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, ct);

    public Task<bool> HasItemsAsync(int categoryId, CancellationToken ct = default) =>
        db.InventoryItems.AnyAsync(i => i.CategoryId == categoryId, ct);

    public async Task AddAsync(Category category, CancellationToken ct = default) =>
        await db.Categories.AddAsync(category, ct);

    public void Remove(Category category) => db.Categories.Remove(category);
}
