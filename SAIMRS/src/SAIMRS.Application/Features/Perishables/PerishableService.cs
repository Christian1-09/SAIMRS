using SAIMRS.Application.Common;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Common;
using SAIMRS.Domain.Entities;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Features.Perishables;

internal sealed class PerishableService(
    IPerishableBatchRepository batches,
    ISettingsRepository settings,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock) : IPerishableService
{
    public async Task<List<PerishableDto>> ListAsync(CancellationToken ct = default)
    {
        var nearDays = (await settings.GetAsync(ct)).NearExpiryDays;
        var today = clock.Today;
        return (await batches.ListAsync(ct))
            .Select(b => new PerishableDto(
                b.Id, b.ItemName, b.BatchNo, b.Quantity, b.Unit, b.PlantOrHarvestDate, b.ExpiryDate,
                b.GetDaysLeft(today), b.GetExpiryStatus(today, nearDays), b.Notes))
            .ToList();
    }

    public async Task<PerishableSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var nearDays = (await settings.GetAsync(ct)).NearExpiryDays;
        var list = await ListAsync(ct);
        return new PerishableSummaryDto(
            list.Count(p => p.Status == ExpiryStatus.Expired),
            list.Count(p => p.Status == ExpiryStatus.Critical),
            list.Count(p => p.Status == ExpiryStatus.NearExpiry),
            nearDays);
    }

    public async Task<Result> SaveAsync(SavePerishableRequest request, CancellationToken ct = default)
    {
        if (request.ExpiryDate is not { } expiry)
            return Result.Failure("Expiry date is required.");

        try
        {
            var now = clock.Now;
            if (request.Id is { } id)
            {
                var batch = await batches.GetByIdAsync(id, ct);
                if (batch is null)
                    return Result.Failure("Batch not found.");

                batch.Update(request.ItemName, request.BatchNo, request.Quantity, request.Unit,
                    request.PlantOrHarvestDate, expiry, request.Notes, now);
            }
            else
            {
                await batches.AddAsync(new PerishableBatch(request.ItemName, request.BatchNo, request.Quantity,
                    request.Unit, request.PlantOrHarvestDate, expiry, request.Notes, now), ct);
            }

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var batch = await batches.GetByIdAsync(id, ct);
        if (batch is null)
            return Result.Failure("Batch not found.");

        batches.Remove(batch);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
