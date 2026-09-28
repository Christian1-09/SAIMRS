using SAIMRS.Application.Common;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Features.Transactions;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Common;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Application.Features.Inventory;

internal sealed class InventoryService(
    IInventoryItemRepository items,
    ICategoryRepository categories,
    ISupplierRepository suppliers,
    IStockTransactionRepository transactions,
    ISettingsRepository settings,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IDateTimeProvider clock) : IInventoryService
{
    public async Task<PagedResult<InventoryItemDto>> SearchAsync(InventoryFilter filter, CancellationToken ct = default) =>
        (await items.SearchAsync(filter, ct)).Map(InventoryItemDto.From);

    public async Task<InventoryItemDetailsDto?> GetDetailsAsync(int id, CancellationToken ct = default)
    {
        var item = await items.GetByIdAsync(id, ct);
        if (item is null)
            return null;

        var history = (await transactions.ListAsync(new TransactionQuery { InventoryItemId = id }, ct))
            .OrderByDescending(t => t.TransactionDate)
            .Take(10)
            .Select(TransactionDto.From)
            .ToList();

        return new InventoryItemDetailsDto(InventoryItemDto.From(item), history);
    }

    public async Task<List<ItemLookupDto>> GetLookupAsync(CancellationToken ct = default) =>
        (await items.ListAllAsync(includeArchived: false, ct))
            .OrderBy(i => i.Name)
            .Select(i => new ItemLookupDto(i.Id, i.Name, i.Quantity, i.Unit, i.UnitCost))
            .ToList();

    public async Task<decimal> GetDefaultThresholdAsync(CancellationToken ct = default) =>
        (await settings.GetAsync(ct)).DefaultLowStockThreshold;

    public async Task<Result> SaveAsync(SaveItemRequest request, CancellationToken ct = default)
    {
        if (request.CategoryId is not { } categoryId)
            return Result.Failure("Please select a category.");

        var category = await categories.GetByIdAsync(categoryId, ct);
        if (category is null)
            return Result.Failure("The selected category was not found.");

        Supplier? supplier = null;
        if (request.SupplierId is { } supplierId)
        {
            supplier = await suppliers.GetByIdAsync(supplierId, ct);
            if (supplier is null)
                return Result.Failure("The selected supplier was not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name) &&
            await items.NameExistsAsync(request.Name.Trim(), request.Id, ct))
            return Result.Failure($"An item named \"{request.Name.Trim()}\" already exists.");

        try
        {
            var now = clock.Now;
            if (request.Id is { } id)
            {
                var item = await items.GetByIdAsync(id, ct);
                if (item is null)
                    return Result.Failure("Item not found.");

                item.UpdateDetails(request.Name, category, supplier, request.Unit,
                    request.LowStockThreshold, request.UnitCost, request.Notes, now);
            }
            else
            {
                if (request.InitialQuantity < 0)
                    return Result.Failure("Initial quantity cannot be negative.");

                var item = new InventoryItem(request.Name, category, supplier, request.Unit,
                    request.LowStockThreshold, request.UnitCost, request.Notes, now);

                if (request.InitialQuantity > 0)
                {
                    item.Receive(request.InitialQuantity, request.UnitCost, supplier,
                        $"OPENING-{now:yyyyMMdd-HHmmss}", "Opening balance", currentUser.DisplayName, now);
                }

                await items.AddAsync(item, ct);
            }

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public Task<Result> ArchiveAsync(int id, CancellationToken ct = default) =>
        ChangeArchiveStateAsync(id, archive: true, ct);

    public Task<Result> RestoreAsync(int id, CancellationToken ct = default) =>
        ChangeArchiveStateAsync(id, archive: false, ct);

    private async Task<Result> ChangeArchiveStateAsync(int id, bool archive, CancellationToken ct)
    {
        var item = await items.GetByIdAsync(id, ct);
        if (item is null)
            return Result.Failure("Item not found.");

        if (archive) item.Archive(clock.Now);
        else item.Restore();

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
