using SAIMRS.Domain.Common;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Domain.Entities;

/// <summary>A registered farmer (or farm) that receives distributed items.</summary>
public class Beneficiary : BaseEntity, IArchivable
{
    private Beneficiary() { } // for EF Core

    public Beneficiary(
        string code,
        string fullName,
        string barangay,
        string? contactNumber,
        decimal farmSizeHectares,
        CropType cropType,
        BeneficiaryStatus status,
        DateOnly dateRegistered,
        DateTime now)
    {
        Code = Guard.Required(code, "Beneficiary code", 20);
        Update(fullName, barangay, contactNumber, farmSizeHectares, cropType, status, now);
        DateRegistered = dateRegistered;
        CreatedAt = now;
        UpdatedAt = null;
    }

    /// <summary>Human-friendly ID, e.g. BEN-0001.</summary>
    public string Code { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Barangay { get; private set; } = string.Empty;
    public string? ContactNumber { get; private set; }
    public decimal FarmSizeHectares { get; private set; }
    public CropType CropType { get; private set; }
    public BeneficiaryStatus Status { get; private set; }
    public DateOnly DateRegistered { get; private set; }

    public bool IsArchived { get; private set; }
    public DateTime? ArchivedAt { get; private set; }

    /// <summary>Only active, non-archived beneficiaries may receive stock.</summary>
    public bool CanReceiveItems => Status == BeneficiaryStatus.Active && !IsArchived;

    public void Update(
        string fullName,
        string barangay,
        string? contactNumber,
        decimal farmSizeHectares,
        CropType cropType,
        BeneficiaryStatus status,
        DateTime now)
    {
        FullName = Guard.Required(fullName, "Full name", 120);
        Barangay = Guard.Required(barangay, "Barangay", 80);
        ContactNumber = Guard.Optional(contactNumber, "Contact number", 30);
        FarmSizeHectares = Guard.NotNegative(farmSizeHectares, "Farm size");
        CropType = cropType;
        Status = status;
        Touch(now);
    }

    public void Archive(DateTime now)
    {
        IsArchived = true;
        ArchivedAt = now;
    }

    public void Restore()
    {
        IsArchived = false;
        ArchivedAt = null;
    }
}
