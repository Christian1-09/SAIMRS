using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Features.Auth;

/// <summary>First-run registration of the single admin account.</summary>
public sealed class SetupAdminRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public SecurityQuestion? SecurityQuestion { get; set; }
    public string SecurityAnswer { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class ResetPasswordRequest
{
    public string Username { get; set; } = string.Empty;
    public string SecurityAnswer { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>Who is signed in. Kept by the Desktop layer while the app is open.</summary>
public sealed record AdminSession(
    int Id,
    string FirstName,
    string LastName,
    string Username,
    string Initials,
    string? PhotoDataUrl,
    DateTime? LastLoginAt)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public sealed record SecurityQuestionOption(SecurityQuestion Value, string Text);
