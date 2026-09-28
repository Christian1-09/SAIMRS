using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Features.Perishables;

public sealed record PerishableDto(
    int Id,
    string ItemName,
    string BatchNo,
    decimal Quantity,
    string Unit,
    DateOnly? PlantOrHarvestDate,
    DateOnly ExpiryDate,
    int DaysLeft,
    ExpiryStatus Status,
    string? Notes);

public sealed record PerishableSummaryDto(int Expired, int Critical, int NearExpiry, int NearExpiryDays);

public sealed class SavePerishableRequest
{
    public int? Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string BatchNo { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "kg";
    public DateOnly? PlantOrHarvestDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}
