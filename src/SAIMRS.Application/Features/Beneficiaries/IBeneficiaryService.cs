using SAIMRS.Application.Common;
using SAIMRS.Application.Repositories;

namespace SAIMRS.Application.Features.Beneficiaries;

public interface IBeneficiaryService
{
    Task<List<BeneficiaryDto>> ListAsync(BeneficiaryFilter filter, CancellationToken ct = default);

    Task<BeneficiaryStatsDto> GetStatsAsync(CancellationToken ct = default);

    Task<List<string>> GetBarangaysAsync(CancellationToken ct = default);

    /// <summary>Active beneficiaries who can receive items (for the Stock OUT form).</summary>
    Task<List<LookupItem>> GetLookupAsync(CancellationToken ct = default);

    /// <summary>Details plus everything the beneficiary has received.</summary>
    Task<BeneficiaryProfileDto?> GetProfileAsync(int id, CancellationToken ct = default);

    Task<Result> SaveAsync(SaveBeneficiaryRequest request, CancellationToken ct = default);

    Task<Result> ArchiveAsync(int id, CancellationToken ct = default);

    Task<Result> RestoreAsync(int id, CancellationToken ct = default);
}
