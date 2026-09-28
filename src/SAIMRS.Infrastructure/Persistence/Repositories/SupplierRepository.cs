using Microsoft.EntityFrameworkCore;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Infrastructure.Persistence.Repositories;

internal sealed class SupplierRepository(AppDbContext db) : ISupplierRepository
{
    public Task<List<Supplier>> ListAsync(bool archived, CancellationToken ct = default) =>
        db.Suppliers.AsNoTracking()
            .Where(s => s.IsArchived == archived)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

    public Task<Supplier?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Suppliers.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct = default) =>
        db.Suppliers.AnyAsync(s => s.Name == name && (excludeId == null || s.Id != excludeId), ct);

    public async Task AddAsync(Supplier supplier, CancellationToken ct = default) =>
        await db.Suppliers.AddAsync(supplier, ct);
}
