using SAIMRS.Application.Common.Interfaces;
using SAIMRS.Infrastructure.Services;

namespace SAIMRS.Infrastructure.Storage;

/// <summary>Copies the chosen photo into %LocalAppData%\SAIMRS\photos.</summary>
internal sealed class LocalProfilePhotoStorage(AppPaths paths) : IProfilePhotoStorage
{
    public async Task<string> SaveAsync(byte[] content, string extension, CancellationToken ct = default)
    {
        Directory.CreateDirectory(paths.PhotosDirectory);
        var fileName = $"profile_{Guid.NewGuid():N}{extension}";
        await File.WriteAllBytesAsync(Path.Combine(paths.PhotosDirectory, fileName), content, ct);
        return fileName;
    }

    public async Task<byte[]?> ReadAsync(string fileName, CancellationToken ct = default)
    {
        var path = SafePath(fileName);
        return path is not null && File.Exists(path) ? await File.ReadAllBytesAsync(path, ct) : null;
    }

    public void Delete(string fileName)
    {
        var path = SafePath(fileName);
        try
        {
            if (path is not null && File.Exists(path))
                File.Delete(path);
        }
        catch (IOException)
        {
            // The old photo may be in use; leaving it behind is harmless.
        }
    }

    /// <summary>Only allow plain file names inside the photos folder.</summary>
    private string? SafePath(string fileName) =>
        string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileName(fileName)
            ? null
            : Path.Combine(paths.PhotosDirectory, fileName);
}
