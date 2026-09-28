using SAIMRS.Application.Common;
using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Features.Auth;

public interface IAuthService
{
    /// <summary>True when no admin account exists yet (first run).</summary>
    Task<bool> IsSetupRequiredAsync(CancellationToken ct = default);

    IReadOnlyList<SecurityQuestionOption> GetSecurityQuestionOptions();

    Task<Result> RegisterAsync(SetupAdminRequest request, CancellationToken ct = default);

    Task<Result<AdminSession>> LoginAsync(LoginRequest request, CancellationToken ct = default);

    /// <summary>Forgot password, step 1: returns the admin's security question.</summary>
    Task<Result<SecurityQuestionOption>> GetSecurityQuestionAsync(string username, CancellationToken ct = default);

    /// <summary>Forgot password, step 2: checks the answer.</summary>
    Task<Result> VerifySecurityAnswerAsync(string username, string answer, CancellationToken ct = default);

    /// <summary>Forgot password, step 3: sets the new password (the answer is checked again).</summary>
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
}
