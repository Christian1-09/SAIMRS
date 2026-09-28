using SAIMRS.Application.Common;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Application.Repositories;

public interface IInventoryItemRepository
{
    Task<PagedResult<InventoryItem>> SearchAsync(InventoryFilter filter, CancellationToken ct = default);

    /// <summary>Items with their category and supplier (read-only).</summary>
    Task<List<InventoryItem>> ListAllAsync(bool includeArchived = false, CancellationToken ct = default);

    /// <summary>Tracked item with category and supplier, ready for updates.</summary>
    Task<InventoryItem?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct = default);

    Task AddAsync(InventoryItem item, CancellationToken ct = default);
}
