using SAIMRS.Domain.Entities;

namespace SAIMRS.Application.Repositories;

public interface IBeneficiaryRepository
{
    Task<List<Beneficiary>> ListAsync(BeneficiaryFilter filter, CancellationToken ct = default);

    /// <summary>All non-archived beneficiaries (for stats and dropdowns).</summary>
    Task<List<Beneficiary>> ListActiveRecordsAsync(CancellationToken ct = default);

    Task<Beneficiary?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Highest number used in codes like BEN-0007 (returns 7), or 0 when none.</summary>
    Task<int> GetMaxCodeNumberAsync(CancellationToken ct = default);

    Task<List<string>> ListBarangaysAsync(CancellationToken ct = default);

    Task AddAsync(Beneficiary beneficiary, CancellationToken ct = default);
}
