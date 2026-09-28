using SAIMRS.Application.Common;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Repositories;

namespace SAIMRS.Application.Features.Backup;

internal sealed class BackupService(
    IDatabaseBackupService database,
    ISettingsRepository settings,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock) : IBackupService
{
    public async Task<BackupInfoDto> GetInfoAsync(CancellationToken ct = default) =>
        new(database.DatabasePath, database.GetDatabaseSizeBytes(), (await settings.GetAsync(ct)).LastBackupAt);

    public string SuggestBackupFileName() => $"SAIMRS_Backup_{clock.Now:yyyy-MM-dd_HHmm}.db";

    public async Task<Result> BackupAsync(string destinationPath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(destinationPath))
            return Result.Failure("Please choose where to save the backup.");

        try
        {
            // Record the time first so the backup file itself shows when it was made.
            var s = await settings.GetAsync(ct);
            s.RecordBackup(clock.Now);
            await unitOfWork.SaveChangesAsync(ct);

            await database.BackupToAsync(destinationPath, ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Backup failed: {ex.Message}");
        }
    }

    public async Task<Result<string>> RestoreAsync(string sourcePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            return Result.Failure<string>("The selected backup file was not found.");

        if (await database.ValidateBackupFileAsync(sourcePath, ct) is { } error)
            return Result.Failure<string>(error);

        try
        {
            var safetyCopy = await database.RestoreFromAsync(sourcePath, ct);
            return Result.Success(safetyCopy);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>($"Restore failed: {ex.Message}");
        }
    }
}
