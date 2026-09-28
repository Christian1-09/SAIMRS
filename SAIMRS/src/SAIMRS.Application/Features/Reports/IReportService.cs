using SAIMRS.Application.Common;

namespace SAIMRS.Application.Features.Reports;

public interface IReportService
{
    /// <summary>Builds the report data (used for the on-screen preview and for exporting).</summary>
    Task<ReportDocument> BuildAsync(ReportRequest request, CancellationToken ct = default);

    /// <summary>Builds the report and renders it as a PDF or Excel file.</summary>
    Task<Result<ExportedFile>> ExportAsync(ReportRequest request, ExportFormat format, CancellationToken ct = default);
}
