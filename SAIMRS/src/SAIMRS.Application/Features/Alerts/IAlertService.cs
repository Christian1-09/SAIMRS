namespace SAIMRS.Application.Features.Alerts;

public interface IAlertService
{
    /// <summary>Alerts generated from current stock levels, expiry dates and recent deliveries.</summary>
    Task<List<AlertDto>> GetAlertsAsync(CancellationToken ct = default);

    Task<AlertCountsDto> GetCountsAsync(CancellationToken ct = default);
}
