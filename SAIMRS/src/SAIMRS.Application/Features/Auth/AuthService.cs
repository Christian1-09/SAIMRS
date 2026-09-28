using SAIMRS.Application.Common;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Common;
using SAIMRS.Domain.Entities;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Features.Auth;

internal sealed class AuthService(
    IAdminRepository admins,
    IUnitOfWork unitOfWork,
    IPasswordHasher hasher,
    IDateTimeProvider clock,
    AdminSessionFactory sessionFactory) : IAuthService
{
    private const string InvalidCredentials = "Invalid username or password.";

    public async Task<bool> IsSetupRequiredAsync(CancellationToken ct = default) =>
        !await admins.ExistsAsync(ct);

    public IReadOnlyList<SecurityQuestionOption> GetSecurityQuestionOptions() =>
        Enum.GetValues<SecurityQuestion>()
            .Select(q => new SecurityQuestionOption(q, q.ToQuestionText()))
            .ToList();

    public async Task<Result> RegisterAsync(SetupAdminRequest request, CancellationToken ct = default)
    {
        if (await admins.ExistsAsync(ct))
            return Result.Failure("An admin account already exists. Please log in.");

        if (string.IsNullOrWhiteSpace(request.Username))
            return Result.Failure("Username is required.");
        if (PasswordPolicy.Validate(request.Password, request.ConfirmPassword) is { } passwordError)
            return Result.Failure(passwordError);
        if (request.SecurityQuestion is null)
            return Result.Failure("Please choose a security question.");
        if (string.IsNullOrWhiteSpace(request.SecurityAnswer))
            return Result.Failure("Please enter the answer to your security question.");

        try
        {
            var admin = new AdminAccount(
                request.FirstName,
                request.LastName,
                request.Username,
                hasher.Hash(request.Password),
                request.SecurityQuestion.Value,
                hasher.Hash(PasswordPolicy.NormalizeAnswer(request.SecurityAnswer)),
                clock.Now);

            await admins.AddAsync(admin, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result<AdminSession>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password))
            return Result.Failure<AdminSession>("Please enter your username and password.");

        // There is only one account, so failed attempts are always counted against it.
        var admin = await admins.GetAsync(ct);
        if (admin is null)
            return Result.Failure<AdminSession>("No admin account exists yet. Please complete the setup.");

        var now = clock.Now;
        if (admin.IsLoginLockedOut(now))
            return Result.Failure<AdminSession>(LockedMessage(admin.LoginLockoutUntil!.Value, now));

        var usernameMatches = string.Equals(admin.Username, request.Username.Trim(), StringComparison.OrdinalIgnoreCase);
        if (!usernameMatches || !hasher.Verify(request.Password, admin.PasswordHash))
        {
            var lockedNow = admin.RecordFailedLogin(now);
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Failure<AdminSession>(lockedNow
                ? $"Too many failed attempts. Login is locked for {AdminAccount.LockoutDuration.TotalMinutes:0} minutes."
                : $"{InvalidCredentials} {AdminAccount.MaxFailedAttempts - admin.FailedLoginAttempts} attempt(s) left.");
        }

        admin.RecordSuccessfulLogin(now);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(await sessionFactory.CreateAsync(admin, ct));
    }

    public async Task<Result<SecurityQuestionOption>> GetSecurityQuestionAsync(string username, CancellationToken ct = default)
    {
        var (admin, error) = await FindForRecoveryAsync(username, ct);
        if (admin is null)
            return Result.Failure<SecurityQuestionOption>(error!);

        var q = admin.SecurityQuestion;
        return Result.Success(new SecurityQuestionOption(q, q.ToQuestionText()));
    }

    public async Task<Result> VerifySecurityAnswerAsync(string username, string answer, CancellationToken ct = default)
    {
        var (admin, error) = await FindForRecoveryAsync(username, ct);
        if (admin is null)
            return Result.Failure(error!);

        return await CheckAnswerAsync(admin, answer, ct);
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var (admin, error) = await FindForRecoveryAsync(request.Username, ct);
        if (admin is null)
            return Result.Failure(error!);

        var answerCheck = await CheckAnswerAsync(admin, request.SecurityAnswer, ct);
        if (answerCheck.IsFailure)
            return answerCheck;

        if (PasswordPolicy.Validate(request.NewPassword, request.ConfirmPassword) is { } passwordError)
            return Result.Failure(passwordError);

        admin.ChangePassword(hasher.Hash(request.NewPassword), clock.Now);
        admin.ResetLockouts();
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<(AdminAccount? Admin, string? Error)> FindForRecoveryAsync(string username, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(username))
            return (null, "Please enter your username.");

        var admin = await admins.GetByUsernameAsync(username.Trim(), ct);
        if (admin is null)
            return (null, "No account was found with that username.");

        var now = clock.Now;
        if (admin.IsRecoveryLockedOut(now))
            return (null, LockedMessage(admin.RecoveryLockoutUntil!.Value, now));

        return (admin, null);
    }

    private async Task<Result> CheckAnswerAsync(AdminAccount admin, string answer, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(answer))
            return Result.Failure("Please enter your answer.");

        if (hasher.Verify(PasswordPolicy.NormalizeAnswer(answer), admin.SecurityAnswerHash))
        {
            admin.RecordSuccessfulRecovery();
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }

        var lockedNow = admin.RecordFailedRecovery(clock.Now);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Failure(lockedNow
            ? $"Too many wrong answers. Password recovery is locked for {AdminAccount.LockoutDuration.TotalMinutes:0} minutes."
            : $"The answer is incorrect. {AdminAccount.MaxFailedAttempts - admin.FailedRecoveryAttempts} attempt(s) left.");
    }

    private static string LockedMessage(DateTime until, DateTime now)
    {
        var minutes = Math.Max(1, (int)Math.Ceiling((until - now).TotalMinutes));
        return $"Too many failed attempts. Please try again in {minutes} minute(s).";
    }
}
