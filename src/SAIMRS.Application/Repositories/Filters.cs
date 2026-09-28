using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Repositories;

public enum InventorySort
{
    Name = 1,
    QuantityLowToHigh = 2,
    RecentlyUpdated = 3
}

public sealed record InventoryFilter
{
    public int? CategoryId { get; init; }
    public StockStatus? Status { get; init; }
    public string? Search { get; init; }
    public InventorySort Sort { get; init; } = InventorySort.Name;

    /// <summary>False = active items, true = archived items only.</summary>
    public bool ShowArchived { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public sealed record TransactionFilter
{
    public TransactionType? Type { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 15;
}

public sealed record BeneficiaryFilter
{
    public BeneficiaryStatus? Status { get; init; }
    public string? Barangay { get; init; }
    public CropType? CropType { get; init; }
    public string? Search { get; init; }

    /// <summary>False = active records, true = archived records only.</summary>
    public bool ShowArchived { get; init; }
}

/// <summary>Criteria for pulling transactions into a report.</summary>
public sealed record TransactionQuery
{
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public int? CategoryId { get; init; }
    public TransactionType? Type { get; init; }
    public int? BeneficiaryId { get; init; }
    public int? InventoryItemId { get; init; }
}
