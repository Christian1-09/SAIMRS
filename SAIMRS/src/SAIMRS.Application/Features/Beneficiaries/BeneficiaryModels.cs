using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Features.Beneficiaries;

public sealed record BeneficiaryDto(
    int Id,
    string Code,
    string FullName,
    string Barangay,
    string? ContactNumber,
    decimal FarmSizeHectares,
    CropType CropType,
    BeneficiaryStatus Status,
    DateOnly DateRegistered,
    bool IsArchived);

public sealed record BeneficiaryStatsDto(int Total, int Active, int BarangaysCovered);

public sealed record ReceivedItemDto(
    DateTime Date,
    string ReferenceNo,
    string ItemName,
    decimal Quantity,
    string Unit,
    decimal Value);

public sealed record BeneficiaryProfileDto(
    BeneficiaryDto Beneficiary,
    IReadOnlyList<ReceivedItemDto> ItemsReceived,
    decimal TotalValueReceived);

public sealed class SaveBeneficiaryRequest
{
    public int? Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Barangay { get; set; } = string.Empty;
    public string? ContactNumber { get; set; }
    public decimal FarmSizeHectares { get; set; }
    public CropType CropType { get; set; } = CropType.Rice;
    public BeneficiaryStatus Status { get; set; } = BeneficiaryStatus.Active;
}
