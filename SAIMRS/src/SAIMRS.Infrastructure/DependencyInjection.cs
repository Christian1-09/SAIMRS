using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Repositories;
using SAIMRS.Infrastructure.Backup;
using SAIMRS.Infrastructure.Persistence;
using SAIMRS.Infrastructure.Persistence.Repositories;
using SAIMRS.Infrastructure.Persistence.Seed;
using SAIMRS.Infrastructure.Reports.Excel;
using SAIMRS.Infrastructure.Reports.Pdf;
using SAIMRS.Infrastructure.Security;
using SAIMRS.Infrastructure.Services;
using SAIMRS.Infrastructure.Storage;

namespace SAIMRS.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers SQLite, repositories and the technical services.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, AppPaths paths)
    {
        paths.EnsureDirectoriesExist();
        services.AddSingleton(paths);

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(paths.ConnectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DbInitializer>();

        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
        services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
        services.AddScoped<IPerishableBatchRepository, PerishableBatchRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IProfilePhotoStorage, LocalProfilePhotoStorage>();
        services.AddSingleton<IDatabaseBackupService, SqliteBackupService>();
        services.AddSingleton<IPdfReportGenerator, QuestPdfReportGenerator>();
        services.AddSingleton<IExcelReportGenerator, ClosedXmlReportGenerator>();

        return services;
    }
}
