namespace SAIMRS.Application.Common.Interfaces;

/// <summary>Stores the admin's profile photo on disk.</summary>
public interface IProfilePhotoStorage
{
    /// <summary>Saves the image and returns the stored file name.</summary>
    Task<string> SaveAsync(byte[] content, string extension, CancellationToken ct = default);

    Task<byte[]?> ReadAsync(string fileName, CancellationToken ct = default);

    void Delete(string fileName);
}
