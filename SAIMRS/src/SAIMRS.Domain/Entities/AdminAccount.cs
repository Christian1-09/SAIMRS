using SAIMRS.Domain.Common;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Domain.Entities;

/// <summary>
/// The single administrator account. Created once on first run (Setup screen).
/// Passwords and the security answer are stored only as hashes.
/// </summary>
public class AdminAccount : BaseEntity
{
    public const int MaxFailedAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);

    private AdminAccount() { } // for EF Core

    public AdminAccount(
        string firstName,
        string lastName,
        string username,
        string passwordHash,
        SecurityQuestion securityQuestion,
        string securityAnswerHash,
        DateTime now)
    {
        SetNames(firstName, lastName);
        Username = Guard.Required(username, "Username", 50);
        PasswordHash = Guard.Required(passwordHash, "Password", 500);
        SecurityQuestion = securityQuestion;
        SecurityAnswerHash = Guard.Required(securityAnswerHash, "Security answer", 500);
        CreatedAt = now;
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public SecurityQuestion SecurityQuestion { get; private set; }
    public string SecurityAnswerHash { get; private set; } = string.Empty;

    /// <summary>File name of the profile photo inside the app's photo folder.</summary>
    public string? ProfilePhotoFileName { get; private set; }

    public DateTime? LastLoginAt { get; private set; }

    public int FailedLoginAttempts { get; private set; }
    public DateTime? LoginLockoutUntil { get; private set; }

    public int FailedRecoveryAttempts { get; private set; }
    public DateTime? RecoveryLockoutUntil { get; private set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string Initials =>
        $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpperInvariant();

    // ── Profile ──────────────────────────────────────────────

    public void UpdateProfile(string firstName, string lastName, string username, DateTime now)
    {
        SetNames(firstName, lastName);
        Username = Guard.Required(username, "Username", 50);
        Touch(now);
    }

    public void ChangePassword(string newPasswordHash, DateTime now)
    {
        PasswordHash = Guard.Required(newPasswordHash, "Password", 500);
        Touch(now);
    }

    public void ChangeSecurityQuestion(SecurityQuestion question, string answerHash, DateTime now)
    {
        SecurityQuestion = question;
        SecurityAnswerHash = Guard.Required(answerHash, "Security answer", 500);
        Touch(now);
    }

    public void SetProfilePhoto(string? fileName, DateTime now)
    {
        ProfilePhotoFileName = fileName;
        Touch(now);
    }

    // ── Login lockout ────────────────────────────────────────

    public bool IsLoginLockedOut(DateTime now) => LoginLockoutUntil is { } until && until > now;

    public void RecordSuccessfulLogin(DateTime now)
    {
        FailedLoginAttempts = 0;
        LoginLockoutUntil = null;
        LastLoginAt = now;
    }

    /// <summary>Counts a failed login. Returns true when this failure triggers a lockout.</summary>
    public bool RecordFailedLogin(DateTime now)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts < MaxFailedAttempts)
            return false;

        FailedLoginAttempts = 0;
        LoginLockoutUntil = now.Add(LockoutDuration);
        return true;
    }

    // ── Password recovery lockout ────────────────────────────

    public bool IsRecoveryLockedOut(DateTime now) => RecoveryLockoutUntil is { } until && until > now;

    public void RecordSuccessfulRecovery()
    {
        FailedRecoveryAttempts = 0;
        RecoveryLockoutUntil = null;
    }

    /// <summary>Counts a wrong security answer. Returns true when this failure triggers a lockout.</summary>
    public bool RecordFailedRecovery(DateTime now)
    {
        FailedRecoveryAttempts++;
        if (FailedRecoveryAttempts < MaxFailedAttempts)
            return false;

        FailedRecoveryAttempts = 0;
        RecoveryLockoutUntil = now.Add(LockoutDuration);
        return true;
    }

    /// <summary>Clears both lockouts, e.g. after a successful password reset.</summary>
    public void ResetLockouts()
    {
        FailedLoginAttempts = 0;
        LoginLockoutUntil = null;
        FailedRecoveryAttempts = 0;
        RecoveryLockoutUntil = null;
    }

    private void SetNames(string firstName, string lastName)
    {
        FirstName = Guard.Required(firstName, "First name", 60);
        LastName = Guard.Required(lastName, "Last name", 60);
    }
}
