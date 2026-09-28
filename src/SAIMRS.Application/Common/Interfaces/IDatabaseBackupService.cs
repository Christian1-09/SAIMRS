namespace SAIMRS.Application.Common.Interfaces;

/// <summary>Backup and restore of the SQLite database file.</summary>
public interface IDatabaseBackupService
{
    string DatabasePath { get; }

    long GetDatabaseSizeBytes();

    /// <summary>Writes a consistent copy of the live database to <paramref name="destinationPath"/>.</summary>
    Task BackupToAsync(string destinationPath, CancellationToken ct = default);

    /// <summary>Checks that the file is a SAIMRS database. Returns an error message or null.</summary>
    Task<string?> ValidateBackupFileAsync(string sourcePath, CancellationToken ct = default);

    /// <summary>
    /// Replaces the live database with the backup. A safety copy of the current database
    /// is kept first. The application must restart afterwards.
    /// Returns the path of the safety copy.
    /// </summary>
    Task<string> RestoreFromAsync(string sourcePath, CancellationToken ct = default);
}
