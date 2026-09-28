using Microsoft.EntityFrameworkCore;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Infrastructure.Persistence.Repositories;

internal sealed class AdminRepository(AppDbContext db) : IAdminRepository
{
    public Task<bool> ExistsAsync(CancellationToken ct = default) =>
        db.AdminAccounts.AnyAsync(ct);

    public Task<AdminAccount?> GetAsync(CancellationToken ct = default) =>
        db.AdminAccounts.OrderBy(a => a.Id).FirstOrDefaultAsync(ct);

    // Username uses the NOCASE collation, so this comparison ignores case.
    public Task<AdminAccount?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        db.AdminAccounts.FirstOrDefaultAsync(a => a.Username == username, ct);

    public async Task AddAsync(AdminAccount admin, CancellationToken ct = default) =>
        await db.AdminAccounts.AddAsync(admin, ct);
}
