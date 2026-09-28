using Microsoft.EntityFrameworkCore;
using SAIMRS.Application.Common;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Infrastructure.Persistence.Repositories;

internal sealed class StockTransactionRepository(AppDbContext db) : IStockTransactionRepository
{
    private IQueryable<StockTransaction> WithDetails() =>
        db.StockTransactions.AsNoTracking()
            .Include(t => t.InventoryItem).ThenInclude(i => i.Category)
            .Include(t => t.Supplier)
            .Include(t => t.Beneficiary);

    public async Task<PagedResult<StockTransaction>> SearchAsync(TransactionFilter filter, CancellationToken ct = default)
    {
        var query = WithDetails();

        if (filter.Type is { } type)
            query = query.Where(t => t.Type == type);
        if (filter.From is { } from)
        {
            var start = from.ToDateTime(TimeOnly.MinValue);
            query = query.Where(t => t.TransactionDate >= start);
        }
        if (filter.To is { } to)
        {
            var end = to.AddDays(1).ToDateTime(TimeOnly.MinValue);
            query = query.Where(t => t.TransactionDate < end);
        }
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(t =>
                EF.Functions.Like(t.InventoryItem.Name, pattern) ||
                EF.Functions.Like(t.ReferenceNo, pattern) ||
                (t.Notes != null && EF.Functions.Like(t.Notes, pattern)) ||
                (t.Beneficiary != null && EF.Functions.Like(t.Beneficiary.FullName, pattern)) ||
                (t.Supplier != null && EF.Functions.Like(t.Supplier.Name, pattern)));
        }

        var total = await query.CountAsync(ct);
        var pageSize = Math.Max(1, filter.PageSize);
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        var page = Math.Clamp(filter.Page, 1, totalPages);

        var items = await query
            .OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<StockTransaction>(items, total, page, pageSize);
    }

    public Task<List<StockTransaction>> ListAsync(TransactionQuery q, CancellationToken ct = default)
    {
        var query = WithDetails();

        if (q.From is { } from)
        {
            var start = from.ToDateTime(TimeOnly.MinValue);
            query = query.Where(t => t.TransactionDate >= start);
        }
        if (q.To is { } to)
        {
            var end = to.AddDays(1).ToDateTime(TimeOnly.MinValue);
            query = query.Where(t => t.TransactionDate < end);
        }
        if (q.Type is { } type)
            query = query.Where(t => t.Type == type);
        if (q.CategoryId is { } categoryId)
            query = query.Where(t => t.InventoryItem.CategoryId == categoryId);
        if (q.BeneficiaryId is { } beneficiaryId)
            query = query.Where(t => t.BeneficiaryId == beneficiaryId);
        if (q.InventoryItemId is { } itemId)
            query = query.Where(t => t.InventoryItemId == itemId);

        return query.OrderBy(t => t.TransactionDate).ThenBy(t => t.Id).ToListAsync(ct);
    }

    public Task<List<StockTransaction>> ListRecentAsync(int count, CancellationToken ct = default) =>
        WithDetails()
            .OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.Id)
            .Take(count)
            .ToListAsync(ct);
}
