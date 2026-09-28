namespace SAIMRS.Application.Features.Suppliers;

public sealed record SupplierDto(
    int Id,
    string Name,
    string? ContactPerson,
    string? Phone,
    string? ItemsSupplied,
    DateOnly? LastDeliveryDate,
    bool IsArchived);

public sealed class SaveSupplierRequest
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? ItemsSupplied { get; set; }
}

