using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using SAIMRS.Application;
using SAIMRS.Desktop.Services;
using SAIMRS.Infrastructure;
using SAIMRS.Infrastructure.Persistence.Seed;
using SAIMRS.Infrastructure.Services;

namespace SAIMRS.Desktop;

/// <summary>
/// Composition root: builds the dependency-injection container for every layer,
/// prepares the database, then opens the main window.
/// </summary>
// "System.Windows.Application" is written in full because "SAIMRS.Application" is also a namespace.
public partial class App : System.Windows.Application
{
#if DEBUG
    // Debug builds load demo records (only into an empty database) so every screen has data.
    private const bool SeedSampleData = true;
#else
    // Release builds start clean.
    private const bool SeedSampleData = false;
#endif

    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        try
        {
            var services = new ServiceCollection();
            services.AddWpfBlazorWebView();
#if DEBUG
            services.AddBlazorWebViewDeveloperTools();
#endif
            services.AddApplication();
            services.AddInfrastructure(AppPaths.CreateDefault());
            services.AddDesktopServices();

            Services = services.BuildServiceProvider();

            // Run on a background thread so async database work cannot deadlock the UI thread.
            Task.Run(async () =>
            {
                await using var scope = Services.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<DbInitializer>().InitializeAsync(SeedSampleData);
            }).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"SAIMRS could not start.\n\n{ex.GetBaseException().Message}",
                "SAIMRS", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
            return;
        }

        var window = new MainWindow();
        MainWindow = window;
        window.Show();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"An unexpected error occurred:\n\n{e.Exception.GetBaseException().Message}",
            "SAIMRS", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
