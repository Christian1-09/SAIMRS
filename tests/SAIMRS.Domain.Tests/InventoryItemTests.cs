using SAIMRS.Domain.Common;
using SAIMRS.Domain.Entities;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Domain.Tests;

public class InventoryItemTests
{
    private static readonly DateTime Now = new(2026, 9, 25, 9, 0, 0);

    private static InventoryItem NewItem(decimal threshold = 10, decimal? cost = null) =>
        new("Rice Seeds", new Category("Seeds", null, Now), null, "kg", threshold, cost, null, Now);

    private static Beneficiary ActiveFarmer() =>
        new("BEN-0001", "Juan Dela Cruz", "Upper Irasan", null, 1.5m, CropType.Rice, BeneficiaryStatus.Active, DateOnly.FromDateTime(Now), Now);

    [Fact]
    public void Receive_increases_quantity_and_records_transaction()
    {
        var item = NewItem();

        var txn = item.Receive(25, null, null, "RCV-1", null, "Admin", Now);

        Assert.Equal(25, item.Quantity);
        Assert.Equal(TransactionType.In, txn.Type);
        Assert.Equal(25, txn.QuantityChange);
        Assert.Equal(25, txn.BalanceAfter);
        Assert.Single(item.Transactions);
    }

    [Fact]
    public void Issue_more_than_on_hand_is_blocked()
    {
        var item = NewItem();
        item.Receive(5, null, null, "RCV-1", null, "Admin", Now);

        var ex = Assert.Throws<DomainException>(() => item.Issue(6, ActiveFarmer(), "ISS-1", null, "Admin", Now));

        Assert.Contains("Only 5", ex.Message);
        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public void Issue_to_inactive_beneficiary_is_blocked()
    {
        var item = NewItem();
        item.Receive(10, null, null, "RCV-1", null, "Admin", Now);
        var inactive = new Beneficiary("BEN-0002", "Maria Santos", "Lower Irasan", null, 1, CropType.Corn,
            BeneficiaryStatus.Inactive, DateOnly.FromDateTime(Now), Now);

        Assert.Throws<DomainException>(() => item.Issue(1, inactive, "ISS-1", null, "Admin", Now));
    }

    [Fact]
    public void Issue_records_negative_change_and_beneficiary()
    {
        var item = NewItem();
        item.Receive(10, null, null, "RCV-1", null, "Admin", Now);
        var farmer = ActiveFarmer();

        var txn = item.Issue(4, farmer, "ISS-1", null, "Admin", Now);

        Assert.Equal(6, item.Quantity);
        Assert.Equal(-4, txn.QuantityChange);
        Assert.Same(farmer, txn.Beneficiary);
    }

    [Fact]
    public void AdjustToCount_records_the_difference()
    {
        var item = NewItem();
        item.Receive(20, null, null, "RCV-1", null, "Admin", Now);

        var txn = item.AdjustToCount(18, "ADJ-1", "2 bags damaged", "Admin", Now);

        Assert.Equal(18, item.Quantity);
        Assert.Equal(-2, txn.QuantityChange);
        Assert.Equal(TransactionType.Adjust, txn.Type);
    }

    [Fact]
    public void AdjustToCount_requires_a_reason()
    {
        var item = NewItem();
        item.Receive(20, null, null, "RCV-1", null, "Admin", Now);

        Assert.Throws<DomainException>(() => item.AdjustToCount(18, "ADJ-1", " ", "Admin", Now));
    }

    [Fact]
    public void AdjustToCount_with_same_quantity_is_rejected()
    {
        var item = NewItem();
        item.Receive(20, null, null, "RCV-1", null, "Admin", Now);

        Assert.Throws<DomainException>(() => item.AdjustToCount(20, "ADJ-1", "Recount", "Admin", Now));
    }

    [Fact]
    public void Receive_with_cost_uses_weighted_average()
    {
        var item = NewItem(cost: 100);
        item.Receive(10, 100, null, "RCV-1", null, "Admin", Now);   // 10 @ 100

        item.Receive(30, 120, null, "RCV-2", null, "Admin", Now);   // + 30 @ 120

        // (10 x 100 + 30 x 120) / 40 = 115
        Assert.Equal(115m, item.UnitCost);
        Assert.Equal(40 * 115m, item.StockValue);
    }

    [Theory]
    [InlineData(0, 10, StockStatus.OutOfStock)]
    [InlineData(10, 10, StockStatus.LowStock)]
    [InlineData(11, 10, StockStatus.InStock)]
    [InlineData(3, 0, StockStatus.InStock)]
    public void Status_follows_threshold(int quantity, int threshold, StockStatus expected)
    {
        var item = NewItem(threshold);
        if (quantity > 0)
            item.Receive(quantity, null, null, "RCV-1", null, "Admin", Now);

        Assert.Equal(expected, item.GetStatus());
    }

    [Fact]
    public void Archived_item_cannot_move_stock()
    {
        var item = NewItem();
        item.Archive(Now);

        Assert.Throws<DomainException>(() => item.Receive(1, null, null, "RCV-1", null, "Admin", Now));
    }
}
