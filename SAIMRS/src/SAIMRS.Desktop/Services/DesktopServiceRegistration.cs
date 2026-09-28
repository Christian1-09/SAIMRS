using Microsoft.Extensions.DependencyInjection;
using SAIMRS.Application.Common.Interfaces;

namespace SAIMRS.Desktop.Services;

public static class DesktopServiceRegistration
{
    public static IServiceCollection AddDesktopServices(this IServiceCollection services)
    {
        services.AddSingleton<AuthStateService>();
        services.AddSingleton<ICurrentUserService>(sp => sp.GetRequiredService<AuthStateService>());
        services.AddSingleton<ToastService>();
        services.AddSingleton<ConfirmService>();
        services.AddSingleton<AppEvents>();
        services.AddSingleton<PageState>();
        services.AddSingleton<FileDialogService>();
        services.AddSingleton<AppLifetimeService>();
        return services;
    }
}
