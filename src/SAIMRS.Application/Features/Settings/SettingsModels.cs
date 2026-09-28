namespace SAIMRS.Application.Features.Settings;

public sealed class SettingsDto
{
    public string FacilityName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal DefaultLowStockThreshold { get; set; }
    public int NearExpiryDays { get; set; }
    public string? ReportHeaderNote { get; set; }
    public DateTime? LastBackupAt { get; set; }
}
