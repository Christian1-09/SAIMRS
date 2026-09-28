namespace SAIMRS.Domain.Common;

/// <summary>Units of measure offered in the item and perishable forms.</summary>
public static class Units
{
    public static readonly IReadOnlyList<string> All =
        ["kg", "bag", "sack", "liter", "bottle", "pack", "piece"];
}
