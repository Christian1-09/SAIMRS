using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Features.Auth;

namespace SAIMRS.Desktop.Services;

/// <summary>Remembers who is signed in while the app is open.</summary>
public sealed class AuthStateService : ICurrentUserService
{
    public AdminSession? Session { get; private set; }

    public bool IsAuthenticated => Session is not null;

    /// <summary>Raised when the admin signs in/out or updates the profile.</summary>
    public event Action? Changed;

    public string DisplayName => Session?.FullName ?? "Administrator";

    public void SignIn(AdminSession session)
    {
        Session = session;
        Changed?.Invoke();
    }

    public void Update(AdminSession session)
    {
        Session = session;
        Changed?.Invoke();
    }

    public void SignOut()
    {
        Session = null;
        Changed?.Invoke();
    }
}
