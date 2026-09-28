using Microsoft.EntityFrameworkCore;
using SAIMRS.Application.Common;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Entities;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Infrastructure.Persistence.Repositories;

internal sealed class InventoryItemRepository(AppDbContext db) : IInventoryItemRepository
{
    public async Task<PagedResult<InventoryItem>> SearchAsync(InventoryFilter filter, CancellationToken ct = default)
    {
        var query = db.InventoryItems.AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.Supplier)
            .Where(i => i.IsArchived == filter.ShowArchived);

        if (filter.CategoryId is { } categoryId)
            query = query.Where(i => i.CategoryId == categoryId);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(i => EF.Functions.Like(i.Name, pattern) || EF.Functions.Like(i.Category.Name, pattern));
        }

        // Same rules as InventoryItem.GetStatus(), written so SQLite can evaluate them.
        query = filter.Status switch
        {
            StockStatus.OutOfStock => query.Where(i => i.Quantity <= 0),
            StockStatus.LowStock => query.Where(i => i.Quantity > 0 && i.LowStockThreshold > 0 && i.Quantity <= i.LowStockThreshold),
            StockStatus.InStock => query.Where(i => i.Quantity > 0 && (i.LowStockThreshold <= 0 || i.Quantity > i.LowStockThreshold)),
            _ => query
        };

        query = filter.Sort switch
        {
            InventorySort.QuantityLowToHigh => query.OrderBy(i => i.Quantity).ThenBy(i => i.Name),
            InventorySort.RecentlyUpdated => query.OrderByDescending(i => i.UpdatedAt ?? i.CreatedAt),
            _ => query.OrderBy(i => i.Name)
        };

        var total = await query.CountAsync(ct);
        var pageSize = Math.Max(1, filter.PageSize);
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        var page = Math.Clamp(filter.Page, 1, totalPages);

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<InventoryItem>(items, total, page, pageSize);
    }

    public Task<List<InventoryItem>> ListAllAsync(bool includeArchived = false, CancellationToken ct = default) =>
        db.InventoryItems.AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.Supplier)
            .Where(i => includeArchived || !i.IsArchived)
            .OrderBy(i => i.Name)
            .ToListAsync(ct);

    public Task<InventoryItem?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.InventoryItems
            .Include(i => i.Category)
            .Include(i => i.Supplier)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct = default) =>
        db.InventoryItems.AnyAsync(i => i.Name == name && (excludeId == null || i.Id != excludeId), ct);

    public async Task AddAsync(InventoryItem item, CancellationToken ct = default) =>
        await db.InventoryItems.AddAsync(item, ct);
}
