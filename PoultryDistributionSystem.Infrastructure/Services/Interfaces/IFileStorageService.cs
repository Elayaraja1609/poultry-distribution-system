namespace PoultryDistributionSystem.Infrastructure.Services.Interfaces;

/// <summary>
/// File storage service interface for handling file uploads
/// </summary>
public interface IFileStorageService
{
    Task<string> SaveImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<bool> DeleteImageAsync(string filePath, CancellationToken cancellationToken = default);
    string GetImageUrl(string filePath);
    bool IsValidImageFile(string fileName);
    bool IsValidImageSize(long fileSize);
}
