using SAIMRS.Domain.Entities;

namespace SAIMRS.Application.Repositories;

public interface IPerishableBatchRepository
{
    /// <summary>All batches ordered by expiry date (soonest first).</summary>
    Task<List<PerishableBatch>> ListAsync(CancellationToken ct = default);

    Task<PerishableBatch?> GetByIdAsync(int id, CancellationToken ct = default);

    Task AddAsync(PerishableBatch batch, CancellationToken ct = default);

    void Remove(PerishableBatch batch);
}
