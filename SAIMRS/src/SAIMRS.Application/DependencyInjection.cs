using Microsoft.Extensions.DependencyInjection;
using SAIMRS.Application.Features.Alerts;
using SAIMRS.Application.Features.Auth;
using SAIMRS.Application.Features.Backup;
using SAIMRS.Application.Features.Beneficiaries;
using SAIMRS.Application.Features.Categories;
using SAIMRS.Application.Features.Dashboard;
using SAIMRS.Application.Features.Inventory;
using SAIMRS.Application.Features.Perishables;
using SAIMRS.Application.Features.Profile;
using SAIMRS.Application.Features.Reports;
using SAIMRS.Application.Features.Settings;
using SAIMRS.Application.Features.Suppliers;
using SAIMRS.Application.Features.Transactions;

namespace SAIMRS.Application;

public static class DependencyInjection
{
    /// <summary>Registers every use-case service. Called once from the Desktop composition root.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AdminSessionFactory>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IBeneficiaryService, BeneficiaryService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IPerishableService, PerishableService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IBackupService, BackupService>();

        return services;
    }
}
