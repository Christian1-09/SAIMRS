using Microsoft.Data.Sqlite;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Infrastructure.Services;

namespace SAIMRS.Infrastructure.Backup;

/// <summary>
/// Uses SQLite's online backup API, so the copy is always consistent even while
/// the app has the database open.
/// </summary>
internal sealed class SqliteBackupService(AppPaths paths, IDateTimeProvider clock) : IDatabaseBackupService
{
    private static readonly string[] RequiredTables = ["AdminAccounts", "InventoryItems", "StockTransactions", "Beneficiaries"];

    public string DatabasePath => paths.DatabasePath;

    public long GetDatabaseSizeBytes() =>
        File.Exists(paths.DatabasePath) ? new FileInfo(paths.DatabasePath).Length : 0;

    public Task BackupToAsync(string destinationPath, CancellationToken ct = default) =>
        Task.Run(() => Copy(paths.DatabasePath, destinationPath), ct);

    public Task<string?> ValidateBackupFileAsync(string sourcePath, CancellationToken ct = default) =>
        Task.Run<string?>(() =>
        {
            if (Path.GetFullPath(sourcePath).Equals(Path.GetFullPath(paths.DatabasePath), StringComparison.OrdinalIgnoreCase))
                return "This is the database currently in use. Please choose a backup file.";

            try
            {
                using var connection = new SqliteConnection(
                    new SqliteConnectionStringBuilder { DataSource = sourcePath, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
                var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        tables.Add(reader.GetString(0));
                }

                return RequiredTables.All(tables.Contains)
                    ? null
                    : "The selected file is not a SAIMRS backup.";
            }
            catch (SqliteException)
            {
                return "The selected file is not a valid SAIMRS backup (it could not be opened as a database).";
            }
        }, ct);

    public Task<string> RestoreFromAsync(string sourcePath, CancellationToken ct = default) =>
        Task.Run(() =>
        {
            // 1) Keep a safety copy of the current data first.
            Directory.CreateDirectory(paths.SafetyBackupsDirectory);
            var safetyCopy = Path.Combine(paths.SafetyBackupsDirectory, $"BeforeRestore_{clock.Now:yyyy-MM-dd_HHmmss}.db");
            Copy(paths.DatabasePath, safetyCopy);

            // 2) Close pooled connections, then write the backup over the live database.
            SqliteConnection.ClearAllPools();
            Copy(sourcePath, paths.DatabasePath);
            SqliteConnection.ClearAllPools();

            return safetyCopy;
        }, ct);

    private static void Copy(string sourcePath, string destinationPath)
    {
        var destinationFolder = Path.GetDirectoryName(Path.GetFullPath(destinationPath));
        if (!string.IsNullOrEmpty(destinationFolder))
            Directory.CreateDirectory(destinationFolder);

        using var source = new SqliteConnection(
            new SqliteConnectionStringBuilder { DataSource = sourcePath, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
        using var destination = new SqliteConnection(
            new SqliteConnectionStringBuilder { DataSource = destinationPath, Pooling = false }.ToString());

        source.Open();
        destination.Open();
        source.BackupDatabase(destination);
    }
}
