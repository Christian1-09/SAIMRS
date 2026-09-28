using SAIMRS.Domain.Common;

namespace SAIMRS.Domain.Entities;

/// <summary>System-wide settings. Stored as a single row (Id = 1).</summary>
public class SystemSettings : BaseEntity
{
    private SystemSettings() { } // for EF Core

    public static SystemSettings CreateDefault(DateTime now) => new()
    {
        Id = 1,
        FacilityName = "PAIC Upper Irasan",
        Location = "Pres. Manuel A. Roxas, Zamboanga del Norte",
        DefaultLowStockThreshold = 10,
        NearExpiryDays = 30,
        ReportHeaderNote = "Provincial Agri-Industrial Center — Official Inventory System",
        CreatedAt = now
    };

    public string FacilityName { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public decimal DefaultLowStockThreshold { get; private set; }
    public int NearExpiryDays { get; private set; }
    public string? ReportHeaderNote { get; private set; }
    public DateTime? LastBackupAt { get; private set; }

    public void Update(
        string facilityName,
        string location,
        decimal defaultLowStockThreshold,
        int nearExpiryDays,
        string? reportHeaderNote,
        DateTime now)
    {
        if (nearExpiryDays is < 1 or > 365)
            throw new DomainException("Near-expiry alert days must be between 1 and 365.");

        FacilityName = Guard.Required(facilityName, "Facility name", 150);
        Location = Guard.Required(location, "Location", 200);
        DefaultLowStockThreshold = Guard.NotNegative(defaultLowStockThreshold, "Default low stock threshold");
        NearExpiryDays = nearExpiryDays;
        ReportHeaderNote = Guard.Optional(reportHeaderNote, "Report header note", 300);
        Touch(now);
    }

    public void RecordBackup(DateTime now) => LastBackupAt = now;
}
