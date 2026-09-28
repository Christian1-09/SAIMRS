using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Common;

/// <summary>Display text for enums, shared by the UI and the reports.</summary>
public static class Formatting
{
    public static string ToDisplay(this StockStatus status) => status switch
    {
        StockStatus.InStock => "In Stock",
        StockStatus.LowStock => "Low Stock",
        StockStatus.OutOfStock => "Out of Stock",
        _ => status.ToString()
    };

    public static string ToDisplay(this ExpiryStatus status) => status switch
    {
        ExpiryStatus.Safe => "Safe",
        ExpiryStatus.NearExpiry => "Near Expiry",
        ExpiryStatus.Critical => "Critical",
        ExpiryStatus.Expired => "Expired",
        _ => status.ToString()
    };

    public static string ToDisplay(this TransactionType type) => type switch
    {
        TransactionType.In => "IN",
        TransactionType.Out => "OUT",
        TransactionType.Adjust => "ADJUST",
        _ => type.ToString()
    };

    public static string Qty(decimal value) => value.ToString("#,##0.##");

    public static string Peso(decimal value) => "₱" + value.ToString("#,##0.00");

    public static string DaysLeftText(int days) => days switch
    {
        <= 0 => "Expired",
        1 => "1 day",
        _ => $"{days} days"
    };
}
