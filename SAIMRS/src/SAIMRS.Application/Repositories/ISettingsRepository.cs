using SAIMRS.Domain.Entities;

namespace SAIMRS.Application.Repositories;

public interface ISettingsRepository
{
    /// <summary>Returns the settings row (tracked). Creates the default row if it is missing.</summary>
    Task<SystemSettings> GetAsync(CancellationToken ct = default);
}
