using SAIMRS.Application.Features.Alerts;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Desktop.Components.Shared;

/// <summary>Maps statuses to the CSS badge colors used across the UI.</summary>
public static class UiText
{
    public static string BadgeClass(StockStatus status) => status switch
    {
        StockStatus.InStock => "green",
        StockStatus.LowStock => "amber",
        _ => "red"
    };

    public static string BadgeClass(ExpiryStatus status) => status switch
    {
        ExpiryStatus.Safe => "green",
        ExpiryStatus.NearExpiry => "amber",
        _ => "red"
    };

    public static string BadgeClass(TransactionType type) => type switch
    {
        TransactionType.In => "green",
        TransactionType.Out => "red",
        _ => "amber"
    };

    public static string BadgeClass(BeneficiaryStatus status) =>
        status == BeneficiaryStatus.Active ? "green" : "gray";

    public static string AlertClass(AlertSeverity severity) => severity switch
    {
        AlertSeverity.Danger => "danger",
        AlertSeverity.Warning => "warn",
        _ => "info"
    };

    public static string TransactionIcon(TransactionType type) => type switch
    {
        TransactionType.In => "fa-arrow-down",
        TransactionType.Out => "fa-arrow-up",
        _ => "fa-sliders"
    };

    public static string TransactionColor(TransactionType type) => type switch
    {
        TransactionType.In => "var(--green-500)",
        TransactionType.Out => "var(--red-400)",
        _ => "var(--amber-500)"
    };

    public static string SignedQty(decimal value) =>
        (value > 0 ? "+" : "") + value.ToString("#,##0.##");

    public static string RelativeTime(DateTime when, DateTime now)
    {
        var diff = now - when;
        if (diff.TotalMinutes < 1) return "Just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
        if (when.Date == now.Date) return $"Today {when:h:mm tt}";
        if (when.Date == now.Date.AddDays(-1)) return $"Yesterday {when:h:mm tt}";
        return when.ToString("MMM d, yyyy h:mm tt");
    }
}
