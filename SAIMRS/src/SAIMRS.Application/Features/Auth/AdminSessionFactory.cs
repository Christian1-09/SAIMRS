using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Application.Features.Auth;

/// <summary>Builds an <see cref="AdminSession"/>, including the photo as a data URL for the UI.</summary>
internal sealed class AdminSessionFactory(IProfilePhotoStorage photoStorage)
{
    public async Task<AdminSession> CreateAsync(AdminAccount admin, CancellationToken ct = default) =>
        new(admin.Id,
            admin.FirstName,
            admin.LastName,
            admin.Username,
            admin.Initials,
            await GetPhotoDataUrlAsync(admin.ProfilePhotoFileName, ct),
            admin.LastLoginAt);

    public async Task<string?> GetPhotoDataUrlAsync(string? fileName, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(fileName))
            return null;

        var bytes = await photoStorage.ReadAsync(fileName, ct);
        if (bytes is null)
            return null;

        var mime = Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
        return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }
}
