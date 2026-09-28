using System.Diagnostics;

namespace SAIMRS.Desktop.Services;

public sealed class AppLifetimeService
{
    /// <summary>Starts a new copy of the app and closes this one (needed after a restore).</summary>
    public void Restart()
    {
        var app = System.Windows.Application.Current;
        app.Dispatcher.Invoke(() =>
        {
            if (Environment.ProcessPath is { } exe)
                Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
            app.Shutdown();
        });
    }

    /// <summary>Opens a folder in Windows Explorer.</summary>
    public void OpenFolder(string folder)
    {
        if (Directory.Exists(folder))
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{folder}\"") { UseShellExecute = true });
    }
}
