using SAIMRS.Application.Features.Reports;

namespace SAIMRS.Application.Common.Interfaces;

/// <summary>Renders a report to PDF (QuestPDF in Infrastructure).</summary>
public interface IPdfReportGenerator
{
    byte[] Generate(ReportDocument report);
}

/// <summary>Renders a report to Excel (ClosedXML in Infrastructure).</summary>
public interface IExcelReportGenerator
{
    byte[] Generate(ReportDocument report);
}
