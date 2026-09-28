using SAIMRS.Application.Common;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Common;
using SAIMRS.Domain.Entities;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Features.Transactions;

internal sealed class TransactionService(
    IInventoryItemRepository items,
    IStockTransactionRepository transactions,
    ISupplierRepository suppliers,
    IBeneficiaryRepository beneficiaries,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IDateTimeProvider clock) : ITransactionService
{
    public async Task<PagedResult<TransactionDto>> SearchAsync(TransactionFilter filter, CancellationToken ct = default) =>
        (await transactions.SearchAsync(filter, ct)).Map(TransactionDto.From);

    public async Task<List<TransactionDto>> GetRecentAsync(int count, CancellationToken ct = default) =>
        (await transactions.ListRecentAsync(count, ct)).Select(TransactionDto.From).ToList();

    public async Task<Result<TransactionDto>> LogAsync(LogTransactionRequest request, CancellationToken ct = default)
    {
        if (request.ItemId is not { } itemId)
            return Result.Failure<TransactionDto>("Please select an item.");

        var item = await items.GetByIdAsync(itemId, ct);
        if (item is null)
            return Result.Failure<TransactionDto>("The selected item was not found.");

        var now = clock.Now;
        var reference = string.IsNullOrWhiteSpace(request.ReferenceNo)
            ? ReferenceNumber.Generate(request.Type, now)
            : request.ReferenceNo.Trim();

        try
        {
            StockTransaction transaction;
            switch (request.Type)
            {
                case TransactionType.In:
                {
                    if (request.Quantity is not > 0m)
                        return Result.Failure<TransactionDto>("Enter a quantity greater than zero.");

                    Supplier? supplier = null;
                    if (request.SupplierId is { } supplierId)
                    {
                        supplier = await suppliers.GetByIdAsync(supplierId, ct);
                        if (supplier is null)
                            return Result.Failure<TransactionDto>("The selected supplier was not found.");
                    }

                    transaction = item.Receive(request.Quantity.Value, request.UnitCost, supplier,
                        reference, request.Notes, currentUser.DisplayName, now);
                    break;
                }

                case TransactionType.Out:
                {
                    if (request.Quantity is not > 0m)
                        return Result.Failure<TransactionDto>("Enter a quantity greater than zero.");
                    if (request.BeneficiaryId is not { } beneficiaryId)
                        return Result.Failure<TransactionDto>("Every Stock OUT must be issued to a beneficiary. Please select one.");

                    var beneficiary = await beneficiaries.GetByIdAsync(beneficiaryId, ct);
                    if (beneficiary is null)
                        return Result.Failure<TransactionDto>("The selected beneficiary was not found.");

                    transaction = item.Issue(request.Quantity.Value, beneficiary,
                        reference, request.Notes, currentUser.DisplayName, now);
                    break;
                }

                case TransactionType.Adjust:
                {
                    if (request.CountedQuantity is not { } counted)
                        return Result.Failure<TransactionDto>("Enter the quantity you actually counted.");

                    transaction = item.AdjustToCount(counted, reference, request.Notes, currentUser.DisplayName, now);
                    break;
                }

                default:
                    return Result.Failure<TransactionDto>("Unknown transaction type.");
            }

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success(TransactionDto.From(transaction));
        }
        catch (DomainException ex)
        {
            return Result.Failure<TransactionDto>(ex.Message);
        }
    }
}
