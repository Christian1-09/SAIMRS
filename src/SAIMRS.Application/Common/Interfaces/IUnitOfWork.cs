namespace SAIMRS.Application.Common.Interfaces;

/// <summary>
/// Saves all changes made through the repositories in one database transaction
/// (e.g. the stock movement and the updated item quantity together).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
