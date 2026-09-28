using SAIMRS.Application.Features.Alerts;
using SAIMRS.Application.Features.Transactions;

namespace SAIMRS.Application.Features.Dashboard;

public sealed record LowStockItemDto(int Id, string Name, decimal Quantity, decimal Threshold, string Unit)
{
    /// <summary>Stock as a percentage of the threshold (0–100).</summary>
    public int Percent => Threshold <= 0 ? 100 : (int)Math.Clamp(Math.Round(Quantity / Threshold * 100), 0, 100);
}

/// <summary>Number of transactions per type in one month.</summary>
public sealed record MonthlyMovementDto(string Label, int InCount, int OutCount, int AdjustCount);

public sealed record CategoryShareDto(string Name, int ItemCount, double Percent);

public sealed record DashboardDto(
    int TotalItems,
    int CategoryCount,
    int LowStockCount,
    int NearExpiryCount,
    decimal StockValue,
    int ActiveBeneficiaries,
    IReadOnlyList<AlertDto> TopAlerts,
    IReadOnlyList<LowStockItemDto> LowStockItems,
    IReadOnlyList<TransactionDto> RecentActivity,
    IReadOnlyList<MonthlyMovementDto> MonthlyMovement,
    IReadOnlyList<CategoryShareDto> CategoryShare);
