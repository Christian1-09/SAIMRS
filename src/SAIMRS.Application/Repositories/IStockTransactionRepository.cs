using SAIMRS.Application.Common;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Application.Repositories;

/// <summary>
/// Read side for transactions. New transactions are created by <see cref="InventoryItem"/>
/// and saved together with the item through the unit of work.
/// </summary>
public interface IStockTransactionRepository
{
    Task<PagedResult<StockTransaction>> SearchAsync(TransactionFilter filter, CancellationToken ct = default);

    /// <summary>Transactions with item, category, supplier and beneficiary loaded, oldest first.</summary>
    Task<List<StockTransaction>> ListAsync(TransactionQuery query, CancellationToken ct = default);

    Task<List<StockTransaction>> ListRecentAsync(int count, CancellationToken ct = default);
}
