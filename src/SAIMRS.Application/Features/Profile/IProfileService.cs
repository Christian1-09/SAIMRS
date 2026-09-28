using SAIMRS.Application.Common;
using SAIMRS.Application.Features.Auth;

namespace SAIMRS.Application.Features.Profile;

public interface IProfileService
{
    Task<ProfileDto?> GetAsync(CancellationToken ct = default);

    /// <summary>Saves the profile and returns the refreshed session.</summary>
    Task<Result<AdminSession>> UpdateAsync(UpdateProfileRequest request, CancellationToken ct = default);

    Task<Result<AdminSession>> UpdatePhotoAsync(byte[] content, string fileExtension, CancellationToken ct = default);

    Task<Result<AdminSession>> RemovePhotoAsync(CancellationToken ct = default);
}
