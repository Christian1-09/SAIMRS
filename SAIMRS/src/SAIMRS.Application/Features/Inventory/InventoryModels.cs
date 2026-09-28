using SAIMRS.Application.Features.Transactions;
using SAIMRS.Domain.Entities;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Features.Inventory;

public sealed record InventoryItemDto(
    int Id,
    string Name,
    int CategoryId,
    string CategoryName,
    int? SupplierId,
    string? SupplierName,
    decimal Quantity,
    string Unit,
    decimal LowStockThreshold,
    decimal? UnitCost,
    decimal StockValue,
    StockStatus Status,
    string? Notes,
    DateTime LastUpdated,
    bool IsArchived)
{
    internal static InventoryItemDto From(InventoryItem i) => new(
        i.Id,
        i.Name,
        i.CategoryId,
        i.Category?.Name ?? string.Empty,
        i.SupplierId,
        i.Supplier?.Name,
        i.Quantity,
        i.Unit,
        i.LowStockThreshold,
        i.UnitCost,
        i.StockValue,
        i.GetStatus(),
        i.Notes,
        i.UpdatedAt ?? i.CreatedAt,
        i.IsArchived);
}

public sealed record InventoryItemDetailsDto(InventoryItemDto Item, IReadOnlyList<TransactionDto> RecentTransactions);

/// <summary>Item choice for the transaction form (shows stock on hand).</summary>
public sealed record ItemLookupDto(int Id, string Name, decimal Quantity, string Unit, decimal? UnitCost);

public sealed class SaveItemRequest
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public string Unit { get; set; } = "kg";
    public decimal LowStockThreshold { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// Only used when creating an item. Recorded as an "opening balance" Stock IN.
    /// After that, quantity changes only through transactions.
    /// </summary>
    public decimal InitialQuantity { get; set; }
}
