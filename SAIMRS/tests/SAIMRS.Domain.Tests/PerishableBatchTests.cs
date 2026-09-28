using SAIMRS.Domain.Entities;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Domain.Tests;

public class PerishableBatchTests
{
    private static readonly DateOnly Today = new(2026, 9, 25);

    [Theory]
    [InlineData(-1, ExpiryStatus.Expired)]
    [InlineData(0, ExpiryStatus.Expired)]
    [InlineData(7, ExpiryStatus.Critical)]
    [InlineData(8, ExpiryStatus.NearExpiry)]
    [InlineData(30, ExpiryStatus.NearExpiry)]
    [InlineData(31, ExpiryStatus.Safe)]
    public void Expiry_status_by_days_left(int daysLeft, ExpiryStatus expected)
    {
        var batch = new PerishableBatch("Urea", "BATCH-1", 5, "bag", null, Today.AddDays(daysLeft), null, DateTime.Now);

        Assert.Equal(expected, batch.GetExpiryStatus(Today, nearExpiryDays: 30));
        Assert.Equal(daysLeft, batch.GetDaysLeft(Today));
    }
}
