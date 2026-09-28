using SAIMRS.Domain.Common;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Domain.Entities;

/// <summary>
/// An immutable record of one stock movement. Created only by <see cref="InventoryItem"/>.
/// Transactions are never edited or deleted, which keeps a reliable audit trail.
/// </summary>
public class StockTransaction : BaseEntity
{
    private StockTransaction() { } // for EF Core

    internal StockTransaction(
        TransactionType type,
        InventoryItem item,
        decimal quantityChange,
        decimal balanceAfter,
        decimal? unitCost,
        string referenceNo,
        Supplier? supplier,
        Beneficiary? beneficiary,
        string? notes,
        string performedBy,
        DateTime at)
    {
        Type = type;
        InventoryItem = item;
        if (item.Id != 0) InventoryItemId = item.Id;
        QuantityChange = quantityChange;
        BalanceAfter = balanceAfter;
        UnitCost = unitCost;
        ReferenceNo = Guard.Required(referenceNo, "Reference no.", 50);
        Supplier = supplier;
        if (supplier is { Id: not 0 }) SupplierId = supplier.Id;
        Beneficiary = beneficiary;
        if (beneficiary is { Id: not 0 }) BeneficiaryId = beneficiary.Id;
        Notes = Guard.Optional(notes, "Notes", 500);
        PerformedBy = Guard.Required(performedBy, "Performed by", 120);
        TransactionDate = at;
        CreatedAt = at;
    }

    public TransactionType Type { get; private set; }

    public int InventoryItemId { get; private set; }
    public InventoryItem InventoryItem { get; private set; } = null!;

    /// <summary>Signed change: positive for IN, negative for OUT, either for ADJUST.</summary>
    public decimal QuantityChange { get; private set; }

    /// <summary>Item quantity right after this transaction.</summary>
    public decimal BalanceAfter { get; private set; }

    /// <summary>Unit cost at the time of the transaction (snapshot for reports).</summary>
    public decimal? UnitCost { get; private set; }

    public string ReferenceNo { get; private set; } = string.Empty;

    public int? SupplierId { get; private set; }
    public Supplier? Supplier { get; private set; }

    public int? BeneficiaryId { get; private set; }
    public Beneficiary? Beneficiary { get; private set; }

    public string? Notes { get; private set; }
    public string PerformedBy { get; private set; } = string.Empty;
    public DateTime TransactionDate { get; private set; }

    public string Code => $"TXN-{Id:D4}";

    /// <summary>Absolute quantity x snapshot cost.</summary>
    public decimal Value => Math.Abs(QuantityChange) * (UnitCost ?? 0m);
}
