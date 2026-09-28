namespace SAIMRS.Application.Features.Alerts;

public enum AlertSeverity
{
    Danger = 1,
    Warning = 2,
    Info = 3
}

public enum AlertKind
{
    OutOfStock = 1,
    LowStock = 2,
    Expiry = 3,
    StockReceived = 4
}

/// <param name="Icon">Font Awesome icon name, e.g. "fa-triangle-exclamation".</param>
/// <param name="Link">Page to open when the alert is clicked.</param>
public sealed record AlertDto(
    AlertSeverity Severity,
    AlertKind Kind,
    string Icon,
    string Title,
    string Description,
    DateTime? When,
    string Link);

/// <summary>Numbers shown on the sidebar badges and the bell icon.</summary>
public sealed record AlertCountsDto(int LowStock, int Expiring)
{
    public int Total => LowStock + Expiring;
}
