using Microsoft.EntityFrameworkCore;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Infrastructure.Persistence.Repositories;

internal sealed class SettingsRepository(AppDbContext db, IDateTimeProvider clock) : ISettingsRepository
{
    public async Task<SystemSettings> GetAsync(CancellationToken ct = default)
    {
        var settings = await db.SystemSettings.FirstOrDefaultAsync(s => s.Id == 1, ct);
        if (settings is not null)
            return settings;

        settings = SystemSettings.CreateDefault(clock.Now);
        db.SystemSettings.Add(settings);
        await db.SaveChangesAsync(ct);
        return settings;
    }
}
