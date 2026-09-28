using Microsoft.EntityFrameworkCore;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Infrastructure.Persistence.Repositories;

internal sealed class BeneficiaryRepository(AppDbContext db) : IBeneficiaryRepository
{
    public Task<List<Beneficiary>> ListAsync(BeneficiaryFilter filter, CancellationToken ct = default)
    {
        var query = db.Beneficiaries.AsNoTracking().Where(b => b.IsArchived == filter.ShowArchived);

        if (filter.Status is { } status)
            query = query.Where(b => b.Status == status);
        if (!string.IsNullOrWhiteSpace(filter.Barangay))
            query = query.Where(b => b.Barangay == filter.Barangay);
        if (filter.CropType is { } crop)
            query = query.Where(b => b.CropType == crop);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(b => EF.Functions.Like(b.FullName, pattern) || EF.Functions.Like(b.Code, pattern));
        }

        return query.OrderBy(b => b.Code).ToListAsync(ct);
    }

    public Task<List<Beneficiary>> ListActiveRecordsAsync(CancellationToken ct = default) =>
        db.Beneficiaries.AsNoTracking().Where(b => !b.IsArchived).OrderBy(b => b.FullName).ToListAsync(ct);

    public Task<Beneficiary?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Beneficiaries.FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<int> GetMaxCodeNumberAsync(CancellationToken ct = default)
    {
        var codes = await db.Beneficiaries.Select(b => b.Code).ToListAsync(ct);
        return codes
            .Select(c => int.TryParse(c.Replace("BEN-", "", StringComparison.OrdinalIgnoreCase), out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();
    }

    public async Task<List<string>> ListBarangaysAsync(CancellationToken ct = default)
    {
        var names = await db.Beneficiaries.Where(b => !b.IsArchived).Select(b => b.Barangay).ToListAsync(ct);
        return names.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n => n).ToList();
    }

    public async Task AddAsync(Beneficiary beneficiary, CancellationToken ct = default) =>
        await db.Beneficiaries.AddAsync(beneficiary, ct);
}
