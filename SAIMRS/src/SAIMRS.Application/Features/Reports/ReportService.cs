using SAIMRS.Application.Common;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Features.Perishables;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Entities;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Features.Reports;

internal sealed class ReportService(
    IInventoryItemRepository items,
    ICategoryRepository categories,
    IStockTransactionRepository transactions,
    IBeneficiaryRepository beneficiaries,
    IPerishableService perishables,
    ISettingsRepository settings,
    IPdfReportGenerator pdf,
    IExcelReportGenerator excel,
    ICurrentUserService currentUser,
    IDateTimeProvider clock) : IReportService
{
    public async Task<Result<ExportedFile>> ExportAsync(ReportRequest request, ExportFormat format, CancellationToken ct = default)
    {
        try
        {
            var document = await BuildAsync(request, ct);
            return format == ExportFormat.Pdf
                ? Result.Success(new ExportedFile(document.FileBaseName + ".pdf", ".pdf", pdf.Generate(document)))
                : Result.Success(new ExportedFile(document.FileBaseName + ".xlsx", ".xlsx", excel.Generate(document)));
        }
        catch (Exception ex)
        {
            return Result.Failure<ExportedFile>($"Could not create the report: {ex.Message}");
        }
    }

    public async Task<ReportDocument> BuildAsync(ReportRequest request, CancellationToken ct = default)
    {
        var header = await BuildHeaderAsync(request, ct);
        return request.Type switch
        {
            ReportType.InventorySummary => await InventorySummaryAsync(request, header, ct),
            ReportType.StockMovement => await StockMovementAsync(request, header, ct),
            ReportType.LowStock => await LowStockAsync(request, header, ct),
            ReportType.Expiry => await ExpiryAsync(header, ct),
            ReportType.Distribution => await DistributionAsync(request, header, ct),
            ReportType.Beneficiaries => await BeneficiariesAsync(header, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(request))
        };
    }

    // ── Reports ──────────────────────────────────────────────

    private async Task<ReportDocument> InventorySummaryAsync(ReportRequest request, Header h, CancellationToken ct)
    {
        var list = await ItemsForAsync(request.CategoryId, ct);
        var rows = list
            .OrderBy(i => i.Category.Name).ThenBy(i => i.Name)
            .Select(i => new[]
            {
                i.Name, i.Category.Name, Formatting.Qty(i.Quantity), i.Unit, Formatting.Qty(i.LowStockThreshold),
                i.GetStatus().ToDisplay(), i.UnitCost is { } c ? Formatting.Peso(c) : "—", Formatting.Peso(i.StockValue)
            })
            .ToList();

        var totalValue = list.Sum(i => i.StockValue);
        return Build(h, "Inventory Summary", h.CategorySubtitle,
            [
                new("Item", ColumnAlign.Left, 3f), new("Category", ColumnAlign.Left, 1.6f),
                new("Quantity", ColumnAlign.Right), new("Unit", ColumnAlign.Left, 0.7f),
                new("Threshold", ColumnAlign.Right), new("Status"),
                new("Unit Cost", ColumnAlign.Right, 1.1f), new("Value (at cost)", ColumnAlign.Right, 1.3f)
            ],
            rows,
            ["TOTAL", "", "", "", "", "", "", Formatting.Peso(totalValue)],
            [
                new("Total items", list.Count.ToString()),
                new("In stock", list.Count(i => i.GetStatus() == StockStatus.InStock).ToString()),
                new("Low stock", list.Count(i => i.GetStatus() == StockStatus.LowStock).ToString()),
                new("Out of stock", list.Count(i => i.GetStatus() == StockStatus.OutOfStock).ToString()),
                new("Inventory value (at cost)", Formatting.Peso(totalValue))
            ],
            "Inventory_Summary");
    }

    private async Task<ReportDocument> StockMovementAsync(ReportRequest request, Header h, CancellationToken ct)
    {
        var (from, to) = DefaultRange(request);
        var list = await transactions.ListAsync(new TransactionQuery { From = from, To = to, CategoryId = request.CategoryId }, ct);

        var rows = list.Select(t => new[]
        {
            t.TransactionDate.ToString("yyyy-MM-dd HH:mm"), t.ReferenceNo, t.Type.ToDisplay(), t.InventoryItem.Name,
            (t.QuantityChange > 0 ? "+" : "") + Formatting.Qty(t.QuantityChange) + " " + t.InventoryItem.Unit,
            Formatting.Qty(t.BalanceAfter),
            t.Type == TransactionType.Out ? t.Beneficiary?.FullName ?? "" : t.Supplier?.Name ?? "",
            t.Notes ?? ""
        }).ToList();

        return Build(h, "Stock Movement Report", RangeText(from, to) + h.CategorySuffix,
            [
                new("Date", ColumnAlign.Left, 1.4f), new("Reference", ColumnAlign.Left, 1.5f), new("Type", ColumnAlign.Center, 0.8f),
                new("Item", ColumnAlign.Left, 2.2f), new("Qty", ColumnAlign.Right, 1.1f), new("Balance", ColumnAlign.Right, 0.9f),
                new("Supplier / Beneficiary", ColumnAlign.Left, 1.8f), new("Notes", ColumnAlign.Left, 2f)
            ],
            rows,
            null,
            [
                new("Transactions", list.Count.ToString()),
                new("Stock IN", list.Count(t => t.Type == TransactionType.In).ToString()),
                new("Stock OUT", list.Count(t => t.Type == TransactionType.Out).ToString()),
                new("Adjustments", list.Count(t => t.Type == TransactionType.Adjust).ToString())
            ],
            "Stock_Movement");
    }

    private async Task<ReportDocument> LowStockAsync(ReportRequest request, Header h, CancellationToken ct)
    {
        var list = (await ItemsForAsync(request.CategoryId, ct))
            .Where(i => i.GetStatus() != StockStatus.InStock)
            .OrderBy(i => i.Quantity)
            .ToList();

        var rows = list.Select(i => new[]
        {
            i.Name, i.Category.Name, Formatting.Qty(i.Quantity), Formatting.Qty(i.LowStockThreshold),
            Formatting.Qty(Math.Max(0, i.LowStockThreshold - i.Quantity)), i.Unit, i.GetStatus().ToDisplay(), i.Supplier?.Name ?? "—"
        }).ToList();

        return Build(h, "Low Stock Report", h.CategorySubtitle,
            [
                new("Item", ColumnAlign.Left, 2.6f), new("Category", ColumnAlign.Left, 1.5f), new("On Hand", ColumnAlign.Right),
                new("Threshold", ColumnAlign.Right), new("Shortfall", ColumnAlign.Right), new("Unit", ColumnAlign.Left, 0.7f),
                new("Status"), new("Supplier", ColumnAlign.Left, 1.8f)
            ],
            rows,
            null,
            [
                new("Items needing reorder", list.Count.ToString()),
                new("Out of stock", list.Count(i => i.GetStatus() == StockStatus.OutOfStock).ToString())
            ],
            "Low_Stock");
    }

    private async Task<ReportDocument> ExpiryAsync(Header h, CancellationToken ct)
    {
        var list = (await perishables.ListAsync(ct)).Where(p => p.Status != ExpiryStatus.Safe).ToList();

        var rows = list.Select(p => new[]
        {
            p.ItemName, p.BatchNo, Formatting.Qty(p.Quantity) + " " + p.Unit,
            p.PlantOrHarvestDate?.ToString("yyyy-MM-dd") ?? "—", p.ExpiryDate.ToString("yyyy-MM-dd"),
            Formatting.DaysLeftText(p.DaysLeft), p.Status.ToDisplay()
        }).ToList();

        return Build(h, "Expiry Report", $"Batches expired or expiring within {h.NearExpiryDays} days",
            [
                new("Item", ColumnAlign.Left, 2.6f), new("Batch No.", ColumnAlign.Left, 1.3f), new("Quantity", ColumnAlign.Right, 1.2f),
                new("Plant/Harvest", ColumnAlign.Center, 1.2f), new("Expiry", ColumnAlign.Center, 1.2f),
                new("Days Left", ColumnAlign.Right), new("Status", ColumnAlign.Center)
            ],
            rows,
            null,
            [
                new("Expired", list.Count(p => p.Status == ExpiryStatus.Expired).ToString()),
                new("Critical (7 days or less)", list.Count(p => p.Status == ExpiryStatus.Critical).ToString()),
                new("Near expiry", list.Count(p => p.Status == ExpiryStatus.NearExpiry).ToString())
            ],
            "Expiry_Report");
    }

    private async Task<ReportDocument> DistributionAsync(ReportRequest request, Header h, CancellationToken ct)
    {
        var (from, to) = DefaultRange(request);
        var list = await transactions.ListAsync(new TransactionQuery { From = from, To = to, Type = TransactionType.Out }, ct);

        var rows = list.Select(t => new[]
        {
            t.TransactionDate.ToString("yyyy-MM-dd"), t.ReferenceNo, t.Beneficiary?.FullName ?? "", t.Beneficiary?.Barangay ?? "",
            t.InventoryItem.Name, Formatting.Qty(Math.Abs(t.QuantityChange)), t.InventoryItem.Unit, Formatting.Peso(t.Value)
        }).ToList();

        var totalValue = list.Sum(t => t.Value);
        return Build(h, "Distribution Report", RangeText(from, to),
            [
                new("Date", ColumnAlign.Left, 1.1f), new("Reference", ColumnAlign.Left, 1.5f), new("Beneficiary", ColumnAlign.Left, 2f),
                new("Barangay", ColumnAlign.Left, 1.4f), new("Item", ColumnAlign.Left, 2.2f), new("Qty", ColumnAlign.Right, 0.8f),
                new("Unit", ColumnAlign.Left, 0.7f), new("Value (at cost)", ColumnAlign.Right, 1.3f)
            ],
            rows,
            ["TOTAL", "", "", "", "", "", "", Formatting.Peso(totalValue)],
            [
                new("Distributions", list.Count.ToString()),
                new("Beneficiaries served", list.Select(t => t.BeneficiaryId).Distinct().Count().ToString()),
                new("Total value distributed", Formatting.Peso(totalValue))
            ],
            "Distribution");
    }

    private async Task<ReportDocument> BeneficiariesAsync(Header h, CancellationToken ct)
    {
        var list = (await beneficiaries.ListActiveRecordsAsync(ct)).OrderBy(b => b.Code).ToList();

        var rows = list.Select(b => new[]
        {
            b.Code, b.FullName, b.Barangay, b.ContactNumber ?? "—",
            b.FarmSizeHectares > 0 ? Formatting.Qty(b.FarmSizeHectares) + " ha" : "—",
            b.CropType.ToString(), b.Status.ToString(), b.DateRegistered.ToString("yyyy-MM-dd")
        }).ToList();

        return Build(h, "Beneficiary Masterlist", null,
            [
                new("ID", ColumnAlign.Left, 1f), new("Full Name", ColumnAlign.Left, 2.2f), new("Barangay", ColumnAlign.Left, 1.5f),
                new("Contact No.", ColumnAlign.Left, 1.3f), new("Farm Size", ColumnAlign.Right, 0.9f), new("Crop"),
                new("Status"), new("Registered", ColumnAlign.Center, 1.1f)
            ],
            rows,
            null,
            [
                new("Total beneficiaries", list.Count.ToString()),
                new("Active", list.Count(b => b.Status == BeneficiaryStatus.Active).ToString()),
                new("Barangays covered", list.Select(b => b.Barangay.ToUpperInvariant()).Distinct().Count().ToString())
            ],
            "Beneficiary_Masterlist");
    }

    // ── Helpers ──────────────────────────────────────────────

    private sealed record Header(
        string FacilityName, string Location, string? Note, int NearExpiryDays, string? CategoryName)
    {
        public string? CategorySubtitle => CategoryName is null ? "All categories" : $"Category: {CategoryName}";
        public string CategorySuffix => CategoryName is null ? "" : $" · Category: {CategoryName}";
    }

    private async Task<Header> BuildHeaderAsync(ReportRequest request, CancellationToken ct)
    {
        var s = await settings.GetAsync(ct);
        string? categoryName = null;
        if (request.CategoryId is { } id)
            categoryName = (await categories.GetByIdAsync(id, ct))?.Name;
        return new Header(s.FacilityName, s.Location, s.ReportHeaderNote, s.NearExpiryDays, categoryName);
    }

    private async Task<List<InventoryItem>> ItemsForAsync(int? categoryId, CancellationToken ct)
    {
        var list = await items.ListAllAsync(includeArchived: false, ct);
        return categoryId is { } id ? list.Where(i => i.CategoryId == id).ToList() : list;
    }

    private (DateOnly From, DateOnly To) DefaultRange(ReportRequest request)
    {
        var today = clock.Today;
        var from = request.From ?? new DateOnly(today.Year, today.Month, 1);
        var to = request.To ?? today;
        return from <= to ? (from, to) : (to, from);
    }

    private static string RangeText(DateOnly from, DateOnly to) => $"{from:MMM d, yyyy} – {to:MMM d, yyyy}";

    private ReportDocument Build(
        Header h,
        string title,
        string? subtitle,
        IReadOnlyList<ReportColumn> columns,
        IReadOnlyList<string[]> rows,
        string[]? totals,
        IReadOnlyList<ReportSummaryLine> summary,
        string fileName)
    {
        var now = clock.Now;
        return new ReportDocument
        {
            Title = title,
            Subtitle = subtitle,
            FacilityName = h.FacilityName,
            Location = h.Location,
            HeaderNote = h.Note,
            GeneratedAt = now,
            GeneratedBy = currentUser.DisplayName,
            Columns = columns,
            Rows = rows,
            TotalsRow = rows.Count > 0 ? totals : null,
            Summary = summary,
            FileBaseName = $"{fileName}_{now:yyyy-MM-dd_HHmm}"
        };
    }
}
