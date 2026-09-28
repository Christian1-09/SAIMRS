using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Features.Profile;

public sealed record ProfileDto(
    string FirstName,
    string LastName,
    string Username,
    string Initials,
    SecurityQuestion SecurityQuestion,
    string? PhotoDataUrl,
    DateTime? LastLoginAt)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}

/// <summary>
/// Account tab form. The current password is required when changing the
/// username, the password, or the security question.
/// </summary>
public sealed class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    /// <summary>Leave blank to keep the current password.</summary>
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;

    public SecurityQuestion SecurityQuestion { get; set; }

    /// <summary>Leave blank to keep the current question and answer.</summary>
    public string SecurityAnswer { get; set; } = string.Empty;

    public string CurrentPassword { get; set; } = string.Empty;
}
