using SAIMRS.Application.Common;
using SAIMRS.Application.Repositories;

namespace SAIMRS.Application.Features.Inventory;

public interface IInventoryService
{
    Task<PagedResult<InventoryItemDto>> SearchAsync(InventoryFilter filter, CancellationToken ct = default);

    Task<InventoryItemDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default);

    /// <summary>Active items for dropdowns.</summary>
    Task<List<ItemLookupDto>> GetLookupAsync(CancellationToken ct = default);

    Task<decimal> GetDefaultThresholdAsync(CancellationToken ct = default);

    Task<Result> SaveAsync(SaveItemRequest request, CancellationToken ct = default);

    Task<Result> ArchiveAsync(int id, CancellationToken ct = default);

    Task<Result> RestoreAsync(int id, CancellationToken ct = default);
}
