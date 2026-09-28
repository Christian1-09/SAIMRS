using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Features.Reports;

namespace SAIMRS.Infrastructure.Reports.Pdf;

/// <summary>Renders any <see cref="ReportDocument"/> as an A4 landscape PDF.</summary>
internal sealed class QuestPdfReportGenerator : IPdfReportGenerator
{
    private const string Green = "#236B43";
    private const string LightGreen = "#F0FAF3";
    private const string BorderGrey = "#E5E7EB";
    private const string Muted = "#6B7280";

    static QuestPdfReportGenerator()
    {
        // Free Community license (organizations with under USD 1M annual revenue, incl. government/education use).
        QuestPDF.Settings.License = LicenseType.Community;

        // Never fail a report because a font lacks a symbol (e.g. the peso sign); a fallback font is used.
        QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
    }

    public byte[] Generate(ReportDocument report) =>
        Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Segoe UI"));

                page.Header().Element(c => ComposeHeader(c, report));
                page.Content().PaddingTop(10).Element(c => ComposeContent(c, report));
                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text($"{report.FacilityName} · SAIMRS").FontSize(8).FontColor(Muted);
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(8).FontColor(Muted));
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            });
        }).GeneratePdf();

    private static void ComposeHeader(IContainer container, ReportDocument report)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Text(report.FacilityName).FontSize(14).Bold().FontColor(Green);
                    left.Item().Text(report.Location).FontSize(9).FontColor(Muted);
                    if (!string.IsNullOrWhiteSpace(report.HeaderNote))
                        left.Item().Text(report.HeaderNote).FontSize(8).Italic().FontColor(Muted);
                });

                row.ConstantItem(220).AlignRight().Column(right =>
                {
                    right.Item().AlignRight().Text($"Generated: {report.GeneratedAt:MMM d, yyyy h:mm tt}").FontSize(8).FontColor(Muted);
                    right.Item().AlignRight().Text($"Prepared by: {report.GeneratedBy}").FontSize(8).FontColor(Muted);
                });
            });

            column.Item().PaddingTop(10).Text(report.Title).FontSize(16).Bold();
            if (!string.IsNullOrWhiteSpace(report.Subtitle))
                column.Item().Text(report.Subtitle).FontSize(10).FontColor(Muted);

            column.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Green);
        });
    }

    private static void ComposeContent(IContainer container, ReportDocument report)
    {
        container.Column(column =>
        {
            column.Spacing(10);

            if (report.Summary.Count > 0)
            {
                column.Item().Row(row =>
                {
                    row.Spacing(8);
                    foreach (var line in report.Summary)
                    {
                        row.RelativeItem().Background(LightGreen).Border(0.5f).BorderColor(BorderGrey).Padding(6).Column(box =>
                        {
                            box.Item().Text(line.Label).FontSize(8).FontColor(Muted);
                            box.Item().Text(line.Value).FontSize(12).Bold().FontColor(Green);
                        });
                    }
                });
            }

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    foreach (var c in report.Columns)
                        columns.RelativeColumn(c.RelativeWidth);
                });

                table.Header(header =>
                {
                    foreach (var c in report.Columns)
                    {
                        Align(header.Cell().Background(Green).PaddingVertical(5).PaddingHorizontal(4), c.Align)
                            .Text(c.Header).FontSize(8.5f).Bold().FontColor(Colors.White);
                    }
                });

                if (report.Rows.Count == 0)
                {
                    table.Cell().ColumnSpan((uint)report.Columns.Count).Padding(12).AlignCenter()
                        .Text("No records found for this report.").FontColor(Muted).Italic();
                    return;
                }

                for (var r = 0; r < report.Rows.Count; r++)
                {
                    var background = r % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;
                    var row = report.Rows[r];
                    for (var c = 0; c < report.Columns.Count; c++)
                    {
                        var cell = table.Cell().Background(background).BorderBottom(0.5f).BorderColor(BorderGrey)
                            .PaddingVertical(4).PaddingHorizontal(4);
                        Align(cell, report.Columns[c].Align).Text(c < row.Length ? row[c] : string.Empty);
                    }
                }

                if (report.TotalsRow is { } totals)
                {
                    for (var c = 0; c < report.Columns.Count; c++)
                    {
                        var cell = table.Cell().Background(LightGreen).BorderTop(1).BorderColor(Green)
                            .PaddingVertical(5).PaddingHorizontal(4);
                        Align(cell, report.Columns[c].Align).Text(c < totals.Length ? totals[c] : string.Empty).Bold();
                    }
                }
            });
        });
    }

    private static IContainer Align(IContainer container, ColumnAlign align) => align switch
    {
        ColumnAlign.Right => container.AlignRight(),
        ColumnAlign.Center => container.AlignCenter(),
        _ => container.AlignLeft()
    };
}
