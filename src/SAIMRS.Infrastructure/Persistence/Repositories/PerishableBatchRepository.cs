using Microsoft.EntityFrameworkCore;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Infrastructure.Persistence.Repositories;

internal sealed class PerishableBatchRepository(AppDbContext db) : IPerishableBatchRepository
{
    public Task<List<PerishableBatch>> ListAsync(CancellationToken ct = default) =>
        db.PerishableBatches.AsNoTracking().OrderBy(p => p.ExpiryDate).ThenBy(p => p.ItemName).ToListAsync(ct);

    public Task<PerishableBatch?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.PerishableBatches.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(PerishableBatch batch, CancellationToken ct = default) =>
        await db.PerishableBatches.AddAsync(batch, ct);

    public void Remove(PerishableBatch batch) => db.PerishableBatches.Remove(batch);
}
