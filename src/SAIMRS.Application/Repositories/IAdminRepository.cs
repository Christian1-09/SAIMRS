using SAIMRS.Domain.Entities;

namespace SAIMRS.Application.Repositories;

public interface IAdminRepository
{
    Task<bool> ExistsAsync(CancellationToken ct = default);

    /// <summary>Returns the single admin account (tracked for updates).</summary>
    Task<AdminAccount?> GetAsync(CancellationToken ct = default);

    /// <summary>Case-insensitive lookup.</summary>
    Task<AdminAccount?> GetByUsernameAsync(string username, CancellationToken ct = default);

    Task AddAsync(AdminAccount admin, CancellationToken ct = default);
}
