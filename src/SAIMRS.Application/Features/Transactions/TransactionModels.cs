using SAIMRS.Domain.Entities;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Features.Transactions;

public sealed record TransactionDto(
    int Id,
    string Code,
    TransactionType Type,
    int ItemId,
    string ItemName,
    string Unit,
    decimal QuantityChange,
    decimal BalanceAfter,
    decimal? UnitCost,
    decimal Value,
    string ReferenceNo,
    string? SupplierName,
    string? BeneficiaryName,
    string? Notes,
    string PerformedBy,
    DateTime Date)
{
    /// <summary>Supplier for IN, beneficiary for OUT.</summary>
    public string? Party => Type switch
    {
        TransactionType.In => SupplierName,
        TransactionType.Out => BeneficiaryName,
        _ => null
    };

    internal static TransactionDto From(StockTransaction t) => new(
        t.Id,
        t.Code,
        t.Type,
        t.InventoryItemId,
        t.InventoryItem.Name,
        t.InventoryItem.Unit,
        t.QuantityChange,
        t.BalanceAfter,
        t.UnitCost,
        t.Value,
        t.ReferenceNo,
        t.Supplier?.Name,
        t.Beneficiary is null ? null : $"{t.Beneficiary.FullName} ({t.Beneficiary.Code})",
        t.Notes,
        t.PerformedBy,
        t.TransactionDate);
}

/// <summary>
/// Stock movement form.
/// IN: Quantity (+ optional UnitCost and SupplierId).
/// OUT: Quantity and BeneficiaryId (required).
/// ADJUST: CountedQuantity and Notes (the reason, required).
/// </summary>
public sealed class LogTransactionRequest
{
    public TransactionType Type { get; set; } = TransactionType.In;
    public int? ItemId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? CountedQuantity { get; set; }
    public decimal? UnitCost { get; set; }
    public int? SupplierId { get; set; }
    public int? BeneficiaryId { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
}
