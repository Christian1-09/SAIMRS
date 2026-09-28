using SAIMRS.Application.Common;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Common;
using SAIMRS.Domain.Entities;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Features.Beneficiaries;

internal sealed class BeneficiaryService(
    IBeneficiaryRepository beneficiaries,
    IStockTransactionRepository transactions,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock) : IBeneficiaryService
{
    public async Task<List<BeneficiaryDto>> ListAsync(BeneficiaryFilter filter, CancellationToken ct = default) =>
        (await beneficiaries.ListAsync(filter, ct)).Select(ToDto).ToList();

    public async Task<BeneficiaryStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var all = await beneficiaries.ListActiveRecordsAsync(ct);
        return new BeneficiaryStatsDto(
            all.Count,
            all.Count(b => b.Status == BeneficiaryStatus.Active),
            all.Select(b => b.Barangay.ToUpperInvariant()).Distinct().Count());
    }

    public Task<List<string>> GetBarangaysAsync(CancellationToken ct = default) =>
        beneficiaries.ListBarangaysAsync(ct);

    public async Task<List<LookupItem>> GetLookupAsync(CancellationToken ct = default) =>
        (await beneficiaries.ListActiveRecordsAsync(ct))
            .Where(b => b.CanReceiveItems)
            .OrderBy(b => b.FullName)
            .Select(b => new LookupItem(b.Id, $"{b.FullName} ({b.Code}) — {b.Barangay}"))
            .ToList();

    public async Task<BeneficiaryProfileDto?> GetProfileAsync(int id, CancellationToken ct = default)
    {
        var beneficiary = await beneficiaries.GetByIdAsync(id, ct);
        if (beneficiary is null)
            return null;

        var received = (await transactions.ListAsync(
                new TransactionQuery { BeneficiaryId = id, Type = TransactionType.Out }, ct))
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new ReceivedItemDto(
                t.TransactionDate,
                t.ReferenceNo,
                t.InventoryItem.Name,
                Math.Abs(t.QuantityChange),
                t.InventoryItem.Unit,
                t.Value))
            .ToList();

        return new BeneficiaryProfileDto(ToDto(beneficiary), received, received.Sum(r => r.Value));
    }

    public async Task<Result> SaveAsync(SaveBeneficiaryRequest request, CancellationToken ct = default)
    {
        try
        {
            var now = clock.Now;
            if (request.Id is { } id)
            {
                var beneficiary = await beneficiaries.GetByIdAsync(id, ct);
                if (beneficiary is null)
                    return Result.Failure("Beneficiary not found.");

                beneficiary.Update(request.FullName, request.Barangay, request.ContactNumber,
                    request.FarmSizeHectares, request.CropType, request.Status, now);
            }
            else
            {
                var next = await beneficiaries.GetMaxCodeNumberAsync(ct) + 1;
                await beneficiaries.AddAsync(new Beneficiary(
                    $"BEN-{next:D4}",
                    request.FullName,
                    request.Barangay,
                    request.ContactNumber,
                    request.FarmSizeHectares,
                    request.CropType,
                    request.Status,
                    DateOnly.FromDateTime(now),
                    now), ct);
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
        var beneficiary = await beneficiaries.GetByIdAsync(id, ct);
        if (beneficiary is null)
            return Result.Failure("Beneficiary not found.");

        if (archive) beneficiary.Archive(clock.Now);
        else beneficiary.Restore();

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static BeneficiaryDto ToDto(Beneficiary b) => new(
        b.Id, b.Code, b.FullName, b.Barangay, b.ContactNumber, b.FarmSizeHectares,
        b.CropType, b.Status, b.DateRegistered, b.IsArchived);
}
