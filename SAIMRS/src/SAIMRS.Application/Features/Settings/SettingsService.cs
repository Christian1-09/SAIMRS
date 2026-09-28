using SAIMRS.Application.Common;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Common;

namespace SAIMRS.Application.Features.Settings;

internal sealed class SettingsService(
    ISettingsRepository settings,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ISettingsService
{
    public async Task<SettingsDto> GetAsync(CancellationToken ct = default)
    {
        var s = await settings.GetAsync(ct);
        return new SettingsDto
        {
            FacilityName = s.FacilityName,
            Location = s.Location,
            DefaultLowStockThreshold = s.DefaultLowStockThreshold,
            NearExpiryDays = s.NearExpiryDays,
            ReportHeaderNote = s.ReportHeaderNote,
            LastBackupAt = s.LastBackupAt
        };
    }

    public async Task<Result> SaveAsync(SettingsDto dto, CancellationToken ct = default)
    {
        try
        {
            var s = await settings.GetAsync(ct);
            s.Update(dto.FacilityName, dto.Location, dto.DefaultLowStockThreshold,
                dto.NearExpiryDays, dto.ReportHeaderNote, clock.Now);
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
