namespace SAIMRS.Application.Features.Backup;

public sealed record BackupInfoDto(string DatabasePath, long SizeBytes, DateTime? LastBackupAt)
{
    public string SizeText => SizeBytes switch
    {
        < 1024 => $"{SizeBytes} B",
        < 1024 * 1024 => $"{SizeBytes / 1024.0:0.0} KB",
        _ => $"{SizeBytes / (1024.0 * 1024):0.0} MB"
    };
}
