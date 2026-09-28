using SAIMRS.Application.Common;

namespace SAIMRS.Application.Features.Perishables;

public interface IPerishableService
{
    Task<List<PerishableDto>> ListAsync(CancellationToken ct = default);

    Task<PerishableSummaryDto> GetSummaryAsync(CancellationToken ct = default);

    Task<Result> SaveAsync(SavePerishableRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
