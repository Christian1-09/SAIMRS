using SAIMRS.Domain.Common;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Domain.Entities;

/// <summary>
/// A batch of perishable goods tracked by expiry date.
/// Kept independent from inventory items (as in the original prototype).
/// </summary>
public class PerishableBatch : BaseEntity
{
    public const int CriticalDays = 7;

    private PerishableBatch() { } // for EF Core

    public PerishableBatch(
        string itemName,
        string batchNo,
        decimal quantity,
        string unit,
        DateOnly? plantOrHarvestDate,
        DateOnly expiryDate,
        string? notes,
        DateTime now)
    {
        Update(itemName, batchNo, quantity, unit, plantOrHarvestDate, expiryDate, notes, now);
        CreatedAt = now;
        UpdatedAt = null;
    }

    public string ItemName { get; private set; } = string.Empty;
    public string BatchNo { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public DateOnly? PlantOrHarvestDate { get; private set; }
    public DateOnly ExpiryDate { get; private set; }
    public string? Notes { get; private set; }

    public void Update(
        string itemName,
        string batchNo,
        decimal quantity,
        string unit,
        DateOnly? plantOrHarvestDate,
        DateOnly expiryDate,
        string? notes,
        DateTime now)
    {
        if (plantOrHarvestDate is { } planted && planted > expiryDate)
            throw new DomainException("Plant/harvest date cannot be after the expiry date.");

        ItemName = Guard.Required(itemName, "Item name", 150);
        BatchNo = Guard.Required(batchNo, "Batch no.", 50);
        Quantity = Guard.NotNegative(quantity, "Quantity");
        Unit = Guard.Required(unit, "Unit", 20);
        PlantOrHarvestDate = plantOrHarvestDate;
        ExpiryDate = expiryDate;
        Notes = Guard.Optional(notes, "Notes", 500);
        Touch(now);
    }

    public int GetDaysLeft(DateOnly today) => ExpiryDate.DayNumber - today.DayNumber;

    public ExpiryStatus GetExpiryStatus(DateOnly today, int nearExpiryDays)
    {
        var days = GetDaysLeft(today);
        if (days <= 0) return ExpiryStatus.Expired;
        if (days <= CriticalDays) return ExpiryStatus.Critical;
        if (days <= nearExpiryDays) return ExpiryStatus.NearExpiry;
        return ExpiryStatus.Safe;
    }
}
