using SAIMRS.Application.Common;

namespace SAIMRS.Application.Features.Settings;

public interface ISettingsService
{
    Task<SettingsDto> GetAsync(CancellationToken ct = default);

    Task<Result> SaveAsync(SettingsDto settings, CancellationToken ct = default);
}
