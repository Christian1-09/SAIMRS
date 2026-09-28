using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SAIMRS.Infrastructure.Persistence;

/// <summary>
/// Used only by the "dotnet ef" tools when creating migrations.
/// Example (run from the solution folder):
///   dotnet ef migrations add InitialCreate --project src/SAIMRS.Infrastructure --output-dir Persistence/Migrations
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=design-time.db")
            .Options;
        return new AppDbContext(options);
    }
}
