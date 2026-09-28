using Microsoft.EntityFrameworkCore;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AdminAccount> AdminAccounts => Set<AdminAccount>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<PerishableBatch> PerishableBatches => Set<PerishableBatch>();
    public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite has no decimal type, and EF Core cannot compare or sort decimals stored as TEXT.
        // Storing them as REAL (double) keeps filters like "Quantity <= LowStockThreshold" in SQL.
        // (This also applies to nullable decimals.)
        configurationBuilder.Properties<decimal>().HaveConversion<double>();
    }
}
