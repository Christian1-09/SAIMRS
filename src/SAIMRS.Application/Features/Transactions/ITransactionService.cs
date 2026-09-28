using SAIMRS.Application.Common;
using SAIMRS.Application.Repositories;

namespace SAIMRS.Application.Features.Transactions;

public interface ITransactionService
{
    Task<PagedResult<TransactionDto>> SearchAsync(TransactionFilter filter, CancellationToken ct = default);

    Task<List<TransactionDto>> GetRecentAsync(int count, CancellationToken ct = default);

    /// <summary>Records the movement and updates the item's quantity in one save.</summary>
    Task<Result<TransactionDto>> LogAsync(LogTransactionRequest request, CancellationToken ct = default);
}
