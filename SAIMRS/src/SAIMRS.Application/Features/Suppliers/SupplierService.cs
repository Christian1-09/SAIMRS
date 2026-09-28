using SAIMRS.Application.Common;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Common;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Application.Features.Suppliers;

internal sealed class SupplierService(
    ISupplierRepository suppliers,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ISupplierService
{
    public async Task<List<SupplierDto>> ListAsync(bool archived = false, CancellationToken ct = default) =>
        (await suppliers.ListAsync(archived, ct))
            .Select(s => new SupplierDto(s.Id, s.Name, s.ContactPerson, s.Phone, s.ItemsSupplied, s.LastDeliveryDate, s.IsArchived))
            .ToList();

    public async Task<List<LookupItem>> GetLookupAsync(CancellationToken ct = default) =>
        (await suppliers.ListAsync(archived: false, ct))
            .Select(s => new LookupItem(s.Id, s.Name))
            .ToList();

    public async Task<Result> SaveAsync(SaveSupplierRequest request, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(request.Name) &&
            await suppliers.NameExistsAsync(request.Name.Trim(), request.Id, ct))
            return Result.Failure($"A supplier named \"{request.Name.Trim()}\" already exists.");

        try
        {
            if (request.Id is { } id)
            {
                var supplier = await suppliers.GetByIdAsync(id, ct);
                if (supplier is null)
                    return Result.Failure("Supplier not found.");
                supplier.Update(request.Name, request.ContactPerson, request.Phone, request.ItemsSupplied, clock.Now);
            }
            else
            {
                await suppliers.AddAsync(
                    new Supplier(request.Name, request.ContactPerson, request.Phone, request.ItemsSupplied, clock.Now), ct);
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
        var supplier = await suppliers.GetByIdAsync(id, ct);
        if (supplier is null)
            return Result.Failure("Supplier not found.");

        if (archive) supplier.Archive(clock.Now);
        else supplier.Restore();

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
