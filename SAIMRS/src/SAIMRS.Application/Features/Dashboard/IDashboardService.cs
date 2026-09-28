namespace SAIMRS.Application.Features.Dashboard;

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(CancellationToken ct = default);
}
