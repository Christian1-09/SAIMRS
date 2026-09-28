using SAIMRS.Application.Common;
using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Application.Features.Auth;
using SAIMRS.Application.Repositories;
using SAIMRS.Domain.Common;

namespace SAIMRS.Application.Features.Profile;

internal sealed class ProfileService(
    IAdminRepository admins,
    IUnitOfWork unitOfWork,
    IPasswordHasher hasher,
    IProfilePhotoStorage photoStorage,
    IDateTimeProvider clock,
    AdminSessionFactory sessionFactory) : IProfileService
{
    private const long MaxPhotoBytes = 5 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = [".png", ".jpg", ".jpeg", ".gif", ".webp"];

    public async Task<ProfileDto?> GetAsync(CancellationToken ct = default)
    {
        var admin = await admins.GetAsync(ct);
        if (admin is null)
            return null;

        return new ProfileDto(
            admin.FirstName,
            admin.LastName,
            admin.Username,
            admin.Initials,
            admin.SecurityQuestion,
            await sessionFactory.GetPhotoDataUrlAsync(admin.ProfilePhotoFileName, ct),
            admin.LastLoginAt);
    }

    public async Task<Result<AdminSession>> UpdateAsync(UpdateProfileRequest request, CancellationToken ct = default)
    {
        var admin = await admins.GetAsync(ct);
        if (admin is null)
            return Result.Failure<AdminSession>("Admin account not found.");

        var usernameChanged = !string.Equals(admin.Username, request.Username?.Trim(), StringComparison.Ordinal);
        var passwordChanging = !string.IsNullOrEmpty(request.NewPassword);
        var questionChanging = !string.IsNullOrWhiteSpace(request.SecurityAnswer);

        if (usernameChanged || passwordChanging || questionChanging)
        {
            if (string.IsNullOrEmpty(request.CurrentPassword))
                return Result.Failure<AdminSession>(
                    "Enter your current password to change your username, password or security question.");
            if (!hasher.Verify(request.CurrentPassword, admin.PasswordHash))
                return Result.Failure<AdminSession>("Current password is incorrect.");
        }

        if (passwordChanging &&
            PasswordPolicy.Validate(request.NewPassword, request.ConfirmNewPassword) is { } passwordError)
            return Result.Failure<AdminSession>(passwordError);

        try
        {
            var now = clock.Now;
            admin.UpdateProfile(request.FirstName, request.LastName, request.Username ?? string.Empty, now);

            if (passwordChanging)
                admin.ChangePassword(hasher.Hash(request.NewPassword), now);

            if (questionChanging)
                admin.ChangeSecurityQuestion(
                    request.SecurityQuestion,
                    hasher.Hash(PasswordPolicy.NormalizeAnswer(request.SecurityAnswer)),
                    now);

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success(await sessionFactory.CreateAsync(admin, ct));
        }
        catch (DomainException ex)
        {
            return Result.Failure<AdminSession>(ex.Message);
        }
    }

    public async Task<Result<AdminSession>> UpdatePhotoAsync(byte[] content, string fileExtension, CancellationToken ct = default)
    {
        var extension = fileExtension.StartsWith('.') ? fileExtension.ToLowerInvariant() : "." + fileExtension.ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return Result.Failure<AdminSession>("Please choose a PNG, JPG, GIF or WEBP image.");
        if (content.Length == 0)
            return Result.Failure<AdminSession>("The selected file is empty.");
        if (content.Length > MaxPhotoBytes)
            return Result.Failure<AdminSession>("The photo must be 5 MB or smaller.");

        var admin = await admins.GetAsync(ct);
        if (admin is null)
            return Result.Failure<AdminSession>("Admin account not found.");

        var oldFile = admin.ProfilePhotoFileName;
        var newFile = await photoStorage.SaveAsync(content, extension, ct);
        admin.SetProfilePhoto(newFile, clock.Now);
        await unitOfWork.SaveChangesAsync(ct);

        if (!string.IsNullOrEmpty(oldFile))
            photoStorage.Delete(oldFile);

        return Result.Success(await sessionFactory.CreateAsync(admin, ct));
    }

    public async Task<Result<AdminSession>> RemovePhotoAsync(CancellationToken ct = default)
    {
        var admin = await admins.GetAsync(ct);
        if (admin is null)
            return Result.Failure<AdminSession>("Admin account not found.");

        var oldFile = admin.ProfilePhotoFileName;
        admin.SetProfilePhoto(null, clock.Now);
        await unitOfWork.SaveChangesAsync(ct);

        if (!string.IsNullOrEmpty(oldFile))
            photoStorage.Delete(oldFile);

        return Result.Success(await sessionFactory.CreateAsync(admin, ct));
    }
}
