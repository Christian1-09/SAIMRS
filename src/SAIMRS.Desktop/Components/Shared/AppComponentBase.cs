using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using SAIMRS.Application.Features.Reports;
using SAIMRS.Desktop.Services;

namespace SAIMRS.Desktop.Components.Shared;

/// <summary>
/// Base class for pages and components that call Application services.
///
/// In Blazor Hybrid the whole app lives in ONE dependency-injection scope, so a single
/// DbContext would live for hours and collect stale data. Instead, every call made with
/// <c>Use&lt;TService&gt;().Run(...)</c> gets its own short-lived scope (and DbContext):
///
///   var page = await Use&lt;IInventoryService&gt;().Run(s => s.SearchAsync(filter));
/// </summary>
public abstract class AppComponentBase : ComponentBase
{
    [Inject] protected IServiceScopeFactory ScopeFactory { get; set; } = default!;
    [Inject] protected ToastService Toast { get; set; } = default!;
    [Inject] protected ConfirmService Confirm { get; set; } = default!;
    [Inject] protected AppEvents Events { get; set; } = default!;
    [Inject] protected PageState Page { get; set; } = default!;
    [Inject] protected AuthStateService Auth { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected FileDialogService FileDialogs { get; set; } = default!;

    /// <summary>True while a save/load is running (used to disable buttons).</summary>
    protected bool Busy { get; set; }

    /// <summary>Builds a report, asks where to save it, and writes the PDF/Excel file.</summary>
    protected async Task ExportReportAsync(ReportRequest request, ExportFormat format)
    {
        var result = await Use<IReportService>().Run(s => s.ExportAsync(request, format));
        if (result.IsFailure || result.Value is null)
        {
            Toast.Error(result.Error ?? "The report could not be created.");
            return;
        }

        var filter = format == ExportFormat.Pdf ? FileDialogService.PdfFilter : FileDialogService.ExcelFilter;
        var path = FileDialogs.PickSaveFile(result.Value.FileName, filter, "Save report");
        if (path is null)
            return;

        await File.WriteAllBytesAsync(path, result.Value.Content);
        Toast.Success($"Report saved: {Path.GetFileName(path)}");
    }

    protected ServiceCall<TService> Use<TService>() where TService : notnull => new(ScopeFactory);

    /// <summary>Runs an action with <see cref="Busy"/> set, and shows unexpected errors as a toast.</summary>
    protected async Task RunBusyAsync(Func<Task> action)
    {
        if (Busy) return;
        Busy = true;
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            Toast.Error($"Something went wrong: {ex.GetBaseException().Message}");
        }
        finally
        {
            Busy = false;
        }
    }
}

/// <summary>Resolves a service in a new scope, runs one call, then disposes the scope.</summary>
public readonly struct ServiceCall<TService>(IServiceScopeFactory scopeFactory) where TService : notnull
{
    public async Task<TResult> Run<TResult>(Func<TService, Task<TResult>> work)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        return await work(scope.ServiceProvider.GetRequiredService<TService>());
    }
}
