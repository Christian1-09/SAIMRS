namespace SAIMRS.Application.Features.Reports;

public enum ReportType
{
    InventorySummary = 1,
    StockMovement = 2,
    LowStock = 3,
    Expiry = 4,
    Distribution = 5,
    Beneficiaries = 6
}

public enum ExportFormat
{
    Pdf = 1,
    Excel = 2
}

public enum ColumnAlign
{
    Left = 1,
    Right = 2,
    Center = 3
}

public sealed class ReportRequest
{
    public ReportType Type { get; set; } = ReportType.InventorySummary;

    /// <summary>Used by Stock Movement and Distribution reports.</summary>
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }

    /// <summary>Optional category filter (Inventory Summary, Stock Movement, Low Stock).</summary>
    public int? CategoryId { get; set; }
}

public sealed record ReportColumn(string Header, ColumnAlign Align = ColumnAlign.Left, float RelativeWidth = 1f);

public sealed record ReportSummaryLine(string Label, string Value);

/// <summary>
/// A report as plain tabular data. The same document feeds the on-screen preview,
/// the PDF generator and the Excel generator, so all three always match.
/// </summary>
public sealed class ReportDocument
{
    public required string Title { get; init; }
    public string? Subtitle { get; init; }
    public required string FacilityName { get; init; }
    public required string Location { get; init; }
    public string? HeaderNote { get; init; }
    public required DateTime GeneratedAt { get; init; }
    public required string GeneratedBy { get; init; }

    public required IReadOnlyList<ReportColumn> Columns { get; init; }
    public required IReadOnlyList<string[]> Rows { get; init; }

    /// <summary>Optional totals row (same length as <see cref="Columns"/>).</summary>
    public string[]? TotalsRow { get; init; }

    public IReadOnlyList<ReportSummaryLine> Summary { get; init; } = [];

    /// <summary>File name without extension, e.g. "Inventory_Summary_2026-09-25".</summary>
    public required string FileBaseName { get; init; }
}

public sealed record ExportedFile(string FileName, string Extension, byte[] Content);

public sealed record ReportTypeInfo(ReportType Type, string Title, string Description, string Icon, bool UsesDateRange, bool UsesCategory);

public static class ReportCatalog
{
    public static readonly IReadOnlyList<ReportTypeInfo> All =
    [
        new(ReportType.InventorySummary, "Inventory Summary", "Stock levels, status and value per item", "fa-boxes-stacked", false, true),
        new(ReportType.StockMovement, "Stock Movement", "All IN / OUT / ADJUST transactions in a period", "fa-arrow-right-arrow-left", true, true),
        new(ReportType.LowStock, "Low Stock", "Items at or below their reorder threshold", "fa-triangle-exclamation", false, true),
        new(ReportType.Expiry, "Expiry Report", "Perishable batches that are expired or near expiry", "fa-calendar-xmark", false, false),
        new(ReportType.Distribution, "Distribution", "Items issued to beneficiaries in a period", "fa-hand-holding-heart", true, false),
        new(ReportType.Beneficiaries, "Beneficiary Masterlist", "All registered beneficiaries", "fa-user-group", false, false)
    ];

    public static ReportTypeInfo Get(ReportType type) => All.First(r => r.Type == type);
}
