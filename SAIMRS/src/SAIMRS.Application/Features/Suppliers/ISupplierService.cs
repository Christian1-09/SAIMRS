using SAIMRS.Application.Common;

namespace SAIMRS.Application.Features.Suppliers;

public interface ISupplierService
{
    Task<List<SupplierDto>> ListAsync(bool archived = false, CancellationToken ct = default);

    /// <summary>Active suppliers for dropdowns.</summary>
    Task<List<LookupItem>> GetLookupAsync(CancellationToken ct = default);

    Task<Result> SaveAsync(SaveSupplierRequest request, CancellationToken ct = default);

    Task<Result> ArchiveAsync(int id, CancellationToken ct = default);

    Task<Result> RestoreAsync(int id, CancellationToken ct = default);
}
