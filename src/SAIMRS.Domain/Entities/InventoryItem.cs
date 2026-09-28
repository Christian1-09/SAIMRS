using SAIMRS.Domain.Common;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Domain.Entities;

/// <summary>
/// A stock item. After it is created, its quantity can only change through
/// <see cref="Receive"/>, <see cref="Issue"/> and <see cref="AdjustToCount"/>.
/// Each of those returns the <see cref="StockTransaction"/> that records the change,
/// so the quantity and the transaction history can never get out of sync.
/// </summary>
public class InventoryItem : BaseEntity, IArchivable
{
    private readonly List<StockTransaction> _transactions = [];

    private InventoryItem() { } // for EF Core

    public InventoryItem(
        string name,
        Category category,
        Supplier? supplier,
        string unit,
        decimal lowStockThreshold,
        decimal? unitCost,
        string? notes,
        DateTime now)
    {
        CreatedAt = now;
        UpdateDetails(name, category, supplier, unit, lowStockThreshold, unitCost, notes, now);
    }

    public string Name { get; private set; } = string.Empty;

    public int CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    public int? SupplierId { get; private set; }
    public Supplier? Supplier { get; private set; }

    public decimal Quantity { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public decimal LowStockThreshold { get; private set; }

    /// <summary>Acquisition cost per unit (weighted average). Null when unknown (e.g. harvests, donations).</summary>
    public decimal? UnitCost { get; private set; }

    public string? Notes { get; private set; }

    public bool IsArchived { get; private set; }
    public DateTime? ArchivedAt { get; private set; }

    public IReadOnlyCollection<StockTransaction> Transactions => _transactions;

    /// <summary>Quantity x unit cost. Zero when the cost is unknown.</summary>
    public decimal StockValue => Quantity * (UnitCost ?? 0m);

    public StockStatus GetStatus()
    {
        if (Quantity <= 0)
            return StockStatus.OutOfStock;
        if (LowStockThreshold > 0 && Quantity <= LowStockThreshold)
            return StockStatus.LowStock;
        return StockStatus.InStock;
    }

    public void UpdateDetails(
        string name,
        Category category,
        Supplier? supplier,
        string unit,
        decimal lowStockThreshold,
        decimal? unitCost,
        string? notes,
        DateTime now)
    {
        ArgumentNullException.ThrowIfNull(category);

        Name = Guard.Required(name, "Item name", 150);
        // Set the navigation; EF Core fills in the foreign key. The key is only copied
        // when the related row already exists (Id != 0), e.g. when seeding new rows together.
        Category = category;
        if (category.Id != 0) CategoryId = category.Id;
        Supplier = supplier;
        if (supplier is null || supplier.Id != 0) SupplierId = supplier?.Id;
        Unit = Guard.Required(unit, "Unit", 20);
        LowStockThreshold = Guard.NotNegative(lowStockThreshold, "Low stock threshold");
        UnitCost = unitCost is null ? null : Guard.NotNegative(unitCost.Value, "Unit cost");
        Notes = Guard.Optional(notes, "Notes", 500);
        Touch(now);
    }

    // ── Stock movements ──────────────────────────────────────

    /// <summary>
    /// Stock IN. When a cost is given, the unit cost becomes the weighted average of
    /// the stock on hand and the new delivery.
    /// </summary>
    public StockTransaction Receive(
        decimal quantity,
        decimal? unitCost,
        Supplier? supplier,
        string referenceNo,
        string? notes,
        string performedBy,
        DateTime at)
    {
        EnsureNotArchived();
        Guard.Positive(quantity, "Quantity");
        if (unitCost is < 0m)
            throw new DomainException("Unit cost cannot be negative.");

        if (unitCost is { } newCost)
            UnitCost = CalculateWeightedAverageCost(Quantity, UnitCost, quantity, newCost);

        Quantity += quantity;
        Touch(at);
        supplier?.RecordDelivery(DateOnly.FromDateTime(at));

        return AddTransaction(new StockTransaction(
            TransactionType.In, this, quantity, Quantity, unitCost ?? UnitCost,
            referenceNo, supplier, beneficiary: null, notes, performedBy, at));
    }

    /// <summary>Stock OUT to a beneficiary. Cannot issue more than is on hand.</summary>
    public StockTransaction Issue(
        decimal quantity,
        Beneficiary beneficiary,
        string referenceNo,
        string? notes,
        string performedBy,
        DateTime at)
    {
        ArgumentNullException.ThrowIfNull(beneficiary);
        EnsureNotArchived();
        Guard.Positive(quantity, "Quantity");

        if (!beneficiary.CanReceiveItems)
            throw new DomainException($"{beneficiary.FullName} is not an active beneficiary.");
        if (quantity > Quantity)
            throw new DomainException(
                $"Cannot issue {quantity:0.##} {Unit}. Only {Quantity:0.##} {Unit} of {Name} is on hand.");

        Quantity -= quantity;
        Touch(at);

        return AddTransaction(new StockTransaction(
            TransactionType.Out, this, -quantity, Quantity, UnitCost,
            referenceNo, supplier: null, beneficiary, notes, performedBy, at));
    }

    /// <summary>
    /// Adjustment after a physical count: the admin enters what was actually counted
    /// and the system records the difference. A reason is required.
    /// </summary>
    public StockTransaction AdjustToCount(
        decimal countedQuantity,
        string referenceNo,
        string? reason,
        string performedBy,
        DateTime at)
    {
        EnsureNotArchived();
        Guard.NotNegative(countedQuantity, "Counted quantity");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A reason is required for an adjustment.");

        var difference = countedQuantity - Quantity;
        if (difference == 0)
            throw new DomainException(
                $"The counted quantity matches the current stock ({Quantity:0.##} {Unit}). No adjustment needed.");

        Quantity = countedQuantity;
        Touch(at);

        return AddTransaction(new StockTransaction(
            TransactionType.Adjust, this, difference, Quantity, UnitCost,
            referenceNo, supplier: null, beneficiary: null, reason, performedBy, at));
    }

    public static decimal CalculateWeightedAverageCost(
        decimal currentQuantity, decimal? currentCost, decimal incomingQuantity, decimal incomingCost)
    {
        // No usable history (no stock, or cost never recorded): the new cost wins.
        if (currentQuantity <= 0 || currentCost is null)
            return Math.Round(incomingCost, 2);

        var total = currentQuantity * currentCost.Value + incomingQuantity * incomingCost;
        return Math.Round(total / (currentQuantity + incomingQuantity), 2);
    }

    // ── Archiving ────────────────────────────────────────────

    public void Archive(DateTime now)
    {
        IsArchived = true;
        ArchivedAt = now;
    }

    public void Restore()
    {
        IsArchived = false;
        ArchivedAt = null;
    }

    private void EnsureNotArchived()
    {
        if (IsArchived)
            throw new DomainException($"{Name} is archived. Restore it before recording stock movements.");
    }

    private StockTransaction AddTransaction(StockTransaction transaction)
    {
        _transactions.Add(transaction);
        return transaction;
    }
}
