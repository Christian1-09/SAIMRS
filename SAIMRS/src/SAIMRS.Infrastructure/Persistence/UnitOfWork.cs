using SAIMRS.Application.Common.Interfaces;

namespace SAIMRS.Infrastructure.Persistence;

/// <summary>One SaveChanges call = one SQLite transaction.</summary>
internal sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
