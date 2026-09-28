namespace SAIMRS.Infrastructure.Services;

/// <summary>
/// Where the app keeps its data: %LocalAppData%\SAIMRS
///   saimrs.db               the database
///   photos\                 profile photo
///   backups\before-restore\ automatic safety copies made before a restore
/// </summary>
public sealed class AppPaths
{
    public AppPaths(string rootDirectory)
    {
        RootDirectory = rootDirectory;
        DatabasePath = Path.Combine(rootDirectory, "saimrs.db");
        PhotosDirectory = Path.Combine(rootDirectory, "photos");
        SafetyBackupsDirectory = Path.Combine(rootDirectory, "backups", "before-restore");
    }

    public string RootDirectory { get; }
    public string DatabasePath { get; }
    public string PhotosDirectory { get; }
    public string SafetyBackupsDirectory { get; }

    public string ConnectionString => $"Data Source={DatabasePath}";

    public static AppPaths CreateDefault() =>
        new(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SAIMRS"));

    public void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(RootDirectory);
        Directory.CreateDirectory(PhotosDirectory);
        Directory.CreateDirectory(SafetyBackupsDirectory);
    }
}
