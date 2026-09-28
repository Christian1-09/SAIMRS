using SAIMRS.Domain.Entities;

namespace SAIMRS.Application.Repositories;

public interface ISupplierRepository
{
    Task<List<Supplier>> ListAsync(bool archived, CancellationToken ct = default);

    Task<Supplier?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct = default);

    Task AddAsync(Supplier supplier, CancellationToken ct = default);
}
