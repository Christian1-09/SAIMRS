using SAIMRS.Application.Common;

namespace SAIMRS.Application.Features.Backup;

public interface IBackupService
{
    Task<BackupInfoDto> GetInfoAsync(CancellationToken ct = default);

    /// <summary>Suggested file name, e.g. SAIMRS_Backup_2026-09-25_1930.db</summary>
    string SuggestBackupFileName();

    Task<Result> BackupAsync(string destinationPath, CancellationToken ct = default);

    /// <summary>
    /// Replaces the current data with the backup. Returns the path of the safety copy
    /// of the data that was replaced. The app must restart afterwards.
    /// </summary>
    Task<Result<string>> RestoreAsync(string sourcePath, CancellationToken ct = default);
}
