using System.Globalization;
using ClosedXML.Excel;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Features.Reports;

namespace SAIMRS.Infrastructure.Reports.Excel;

/// <summary>Renders any <see cref="ReportDocument"/> as an .xlsx worksheet.</summary>
internal sealed class ClosedXmlReportGenerator : IExcelReportGenerator
{
    private static readonly XLColor Green = XLColor.FromHtml("#236B43");
    private static readonly XLColor LightGreen = XLColor.FromHtml("#F0FAF3");
    private static readonly XLColor Muted = XLColor.FromHtml("#6B7280");

    public byte[] Generate(ReportDocument report)
    {
        using var workbook = new XLWorkbook();
        var sheetName = report.Title.Length > 31 ? report.Title[..31] : report.Title;
        var ws = workbook.Worksheets.Add(sheetName);
        var columnCount = report.Columns.Count;
        var row = 1;

        // Header block
        WriteMerged(ws, row++, columnCount, report.FacilityName, bold: true, size: 14, color: Green);
        WriteMerged(ws, row++, columnCount, report.Location, color: Muted);
        if (!string.IsNullOrWhiteSpace(report.HeaderNote))
            WriteMerged(ws, row++, columnCount, report.HeaderNote, italic: true, color: Muted);
        row++;
        WriteMerged(ws, row++, columnCount, report.Title, bold: true, size: 13);
        if (!string.IsNullOrWhiteSpace(report.Subtitle))
            WriteMerged(ws, row++, columnCount, report.Subtitle, color: Muted);
        WriteMerged(ws, row++, columnCount,
            $"Generated {report.GeneratedAt:MMM d, yyyy h:mm tt} by {report.GeneratedBy}", italic: true, color: Muted);
        row++;

        // Summary
        foreach (var line in report.Summary)
        {
            ws.Cell(row, 1).Value = line.Label;
            ws.Cell(row, 1).Style.Font.FontColor = Muted;
            SetValue(ws.Cell(row, 2), line.Value);
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            row++;
        }
        if (report.Summary.Count > 0)
            row++;

        // Table header
        var headerRow = row;
        for (var c = 0; c < columnCount; c++)
        {
            var cell = ws.Cell(row, c + 1);
            cell.Value = report.Columns[c].Header;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = Green;
            cell.Style.Alignment.Horizontal = ToExcel(report.Columns[c].Align);
        }
        row++;

        // Rows
        if (report.Rows.Count == 0)
        {
            WriteMerged(ws, row++, columnCount, "No records found for this report.", italic: true, color: Muted);
        }

        foreach (var data in report.Rows)
        {
            for (var c = 0; c < columnCount; c++)
            {
                var cell = ws.Cell(row, c + 1);
                SetValue(cell, c < data.Length ? data[c] : string.Empty);
                cell.Style.Alignment.Horizontal = ToExcel(report.Columns[c].Align);
            }
            row++;
        }

        if (report.TotalsRow is { } totals)
        {
            for (var c = 0; c < columnCount; c++)
            {
                var cell = ws.Cell(row, c + 1);
                SetValue(cell, c < totals.Length ? totals[c] : string.Empty);
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = LightGreen;
                cell.Style.Alignment.Horizontal = ToExcel(report.Columns[c].Align);
            }
            row++;
        }

        var tableRange = ws.Range(headerRow, 1, Math.Max(headerRow, row - 1), columnCount);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Hair;

        ws.SheetView.FreezeRows(headerRow);
        ws.Columns(1, columnCount).AdjustToContents(headerRow, row);
        foreach (var column in ws.Columns(1, columnCount))
        {
            if (column.Width > 60) column.Width = 60;
        }

        ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        ws.PageSetup.FitToPages(1, 0);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>Writes numbers (including "₱1,234.00") as real numbers so Excel can total them.</summary>
    private static void SetValue(IXLCell cell, string text)
    {
        var isPeso = text.StartsWith('₱');
        var numeric = isPeso ? text[1..] : text;

        if (numeric.Length > 0 &&
            decimal.TryParse(numeric, NumberStyles.Number, CultureInfo.InvariantCulture, out var number) &&
            !numeric.StartsWith('+') && !text.Contains('-'))
        {
            cell.Value = (double)number;
            cell.Style.NumberFormat.Format = isPeso ? "\"₱\"#,##0.00" : "#,##0.##";
        }
        else
        {
            cell.Value = text;
        }
    }

    private static void WriteMerged(IXLWorksheet ws, int row, int columnCount, string text,
        bool bold = false, bool italic = false, double size = 11, XLColor? color = null)
    {
        var range = ws.Range(row, 1, row, Math.Max(1, columnCount));
        range.Merge();
        var cell = ws.Cell(row, 1);
        cell.Value = text;
        cell.Style.Font.Bold = bold;
        cell.Style.Font.Italic = italic;
        cell.Style.Font.FontSize = size;
        if (color is not null)
            cell.Style.Font.FontColor = color;
    }

    private static XLAlignmentHorizontalValues ToExcel(ColumnAlign align) => align switch
    {
        ColumnAlign.Right => XLAlignmentHorizontalValues.Right,
        ColumnAlign.Center => XLAlignmentHorizontalValues.Center,
        _ => XLAlignmentHorizontalValues.Left
    };
}
