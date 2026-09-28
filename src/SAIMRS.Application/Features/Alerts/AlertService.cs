using SAIMRS.Application.Common;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Features.Perishables;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Features.Alerts;

internal sealed class AlertService(
    IInventoryItemRepository items,
    IStockTransactionRepository transactions,
    IPerishableService perishables,
    IDateTimeProvider clock) : IAlertService
{
    public async Task<List<AlertDto>> GetAlertsAsync(CancellationToken ct = default)
    {
        var alerts = new List<AlertDto>();

        // 1) Stock levels
        foreach (var item in (await items.ListAllAsync(includeArchived: false, ct)).OrderBy(i => i.Quantity))
        {
            var status = item.GetStatus();
            if (status == StockStatus.OutOfStock)
            {
                alerts.Add(new AlertDto(AlertSeverity.Danger, AlertKind.OutOfStock, "fa-circle-xmark",
                    $"Out of stock: {item.Name}",
                    "No stock left. Record a Stock IN when new supplies arrive.",
                    null, "/inventory"));
            }
            else if (status == StockStatus.LowStock)
            {
                var severity = item.Quantity < item.LowStockThreshold / 2 ? AlertSeverity.Danger : AlertSeverity.Warning;
                alerts.Add(new AlertDto(severity, AlertKind.LowStock, "fa-triangle-exclamation",
                    $"Low stock: {item.Name}",
                    $"Only {Formatting.Qty(item.Quantity)} {item.Unit} left (threshold: {Formatting.Qty(item.LowStockThreshold)} {item.Unit}). Reorder needed.",
                    null, "/inventory"));
            }
        }

        // 2) Expiry
        foreach (var batch in await perishables.ListAsync(ct))
        {
            if (batch.Status == ExpiryStatus.Safe)
                continue;

            var (severity, title) = batch.Status switch
            {
                ExpiryStatus.Expired => (AlertSeverity.Danger, $"Expired: {batch.ItemName}"),
                ExpiryStatus.Critical => (AlertSeverity.Danger, $"Expiring within 7 days: {batch.ItemName}"),
                _ => (AlertSeverity.Warning, $"Near expiry: {batch.ItemName}")
            };

            alerts.Add(new AlertDto(severity, AlertKind.Expiry, "fa-calendar-xmark", title,
                $"Batch {batch.BatchNo} ({Formatting.Qty(batch.Quantity)} {batch.Unit}) — " +
                (batch.DaysLeft <= 0 ? $"expired on {batch.ExpiryDate:MMM d, yyyy}" : $"{Formatting.DaysLeftText(batch.DaysLeft)} left, expires {batch.ExpiryDate:MMM d, yyyy}"),
                null, "/perishables"));
        }

        // 3) Deliveries in the last 24 hours (information only)
        var since = clock.Now.AddDays(-1);
        foreach (var t in (await transactions.ListRecentAsync(20, ct))
                     .Where(t => t.Type == TransactionType.In && t.TransactionDate >= since)
                     .Take(5))
        {
            alerts.Add(new AlertDto(AlertSeverity.Info, AlertKind.StockReceived, "fa-arrow-down",
                $"Stock received: {t.InventoryItem.Name}",
                $"{Formatting.Qty(t.QuantityChange)} {t.InventoryItem.Unit} received" +
                (t.Supplier is null ? "" : $" from {t.Supplier.Name}") + $" (Ref. {t.ReferenceNo})",
                t.TransactionDate, "/transactions"));
        }

        return alerts.OrderBy(a => a.Severity).ToList();
    }

    public async Task<AlertCountsDto> GetCountsAsync(CancellationToken ct = default)
    {
        var lowStock = (await items.ListAllAsync(includeArchived: false, ct))
            .Count(i => i.GetStatus() != StockStatus.InStock);
        var expiring = (await perishables.ListAsync(ct))
            .Count(p => p.Status != ExpiryStatus.Safe);
        return new AlertCountsDto(lowStock, expiring);
    }
}
