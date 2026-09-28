using System.Globalization;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Features.Alerts;
using SAIMRS.Application.Features.Transactions;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Features.Dashboard;

internal sealed class DashboardService(
    IInventoryItemRepository items,
    ICategoryRepository categories,
    IStockTransactionRepository transactions,
    IBeneficiaryRepository beneficiaries,
    IAlertService alerts,
    IDateTimeProvider clock) : IDashboardService
{
    private const int MonthsShown = 6;

    public async Task<DashboardDto> GetAsync(CancellationToken ct = default)
    {
        var allItems = await items.ListAllAsync(includeArchived: false, ct);
        var categoryList = await categories.ListAsync(ct);
        var counts = await alerts.GetCountsAsync(ct);
        var alertList = await alerts.GetAlertsAsync(ct);

        var lowStock = allItems
            .Where(i => i.GetStatus() != StockStatus.InStock)
            .OrderBy(i => i.LowStockThreshold <= 0 ? 0 : i.Quantity / i.LowStockThreshold)
            .Take(6)
            .Select(i => new LowStockItemDto(i.Id, i.Name, i.Quantity, i.LowStockThreshold, i.Unit))
            .ToList();

        var recent = (await transactions.ListRecentAsync(6, ct)).Select(TransactionDto.From).ToList();

        // Monthly movement: count of transactions per month (units differ between items,
        // so counting movements is more meaningful than adding quantities together).
        var today = clock.Today;
        var firstMonth = new DateOnly(today.Year, today.Month, 1).AddMonths(-(MonthsShown - 1));
        var periodTxns = await transactions.ListAsync(new TransactionQuery { From = firstMonth, To = today }, ct);
        var monthly = Enumerable.Range(0, MonthsShown)
            .Select(offset =>
            {
                var month = firstMonth.AddMonths(offset);
                var inMonth = periodTxns
                    .Where(t => t.TransactionDate.Year == month.Year && t.TransactionDate.Month == month.Month)
                    .ToList();
                return new MonthlyMovementDto(
                    month.ToString("MMM", CultureInfo.InvariantCulture),
                    inMonth.Count(t => t.Type == TransactionType.In),
                    inMonth.Count(t => t.Type == TransactionType.Out),
                    inMonth.Count(t => t.Type == TransactionType.Adjust));
            })
            .ToList();

        var total = Math.Max(1, allItems.Count);
        var share = categoryList
            .Select(c => new { c.Name, Count = allItems.Count(i => i.CategoryId == c.Id) })
            .Where(x => x.Count > 0)
            .OrderByDescending(x => x.Count)
            .Select(x => new CategoryShareDto(x.Name, x.Count, Math.Round(x.Count * 100.0 / total, 1)))
            .ToList();

        var activeBeneficiaries = (await beneficiaries.ListActiveRecordsAsync(ct))
            .Count(b => b.Status == BeneficiaryStatus.Active);

        return new DashboardDto(
            allItems.Count,
            categoryList.Count,
            counts.LowStock,
            counts.Expiring,
            allItems.Sum(i => i.StockValue),
            activeBeneficiaries,
            alertList.Take(3).ToList(),
            lowStock,
            recent,
            monthly,
            share);
    }
}
