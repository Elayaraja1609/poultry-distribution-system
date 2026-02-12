using Microsoft.Extensions.Configuration;
using PoultryDistributionSystem.Infrastructure.Services.Interfaces;

namespace PoultryDistributionSystem.Infrastructure.Services;

/// <summary>
/// File storage service implementation for image handling
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly string _imagesFolder;
    private readonly long _maxFileSizeBytes;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public FileStorageService(IConfiguration configuration)
    {
        var basePath = configuration["FileStorage:BasePath"] ?? "wwwroot";
        _imagesFolder = Path.Combine(basePath, "images");
        _maxFileSizeBytes = long.Parse(configuration["FileStorage:MaxFileSizeBytes"] ?? "5242880"); // 5MB default

        // Ensure images directory exists
        if (!Directory.Exists(_imagesFolder))
        {
            Directory.CreateDirectory(_imagesFolder);
        }
    }

    public async Task<string> SaveImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        if (!IsValidImageFile(fileName))
        {
            throw new ArgumentException("Invalid image file type", nameof(fileName));
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var filePath = Path.Combine(_imagesFolder, uniqueFileName);

        using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamWriter, cancellationToken);
        }

        // Return relative path
        return Path.Combine("images", uniqueFileName).Replace("\\", "/");
    }

    public async Task<bool> DeleteImageAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            // Remove "images/" prefix if present
            var relativePath = filePath.StartsWith("images/") ? filePath : $"images/{filePath}";
            var fullPath = Path.Combine("wwwroot", relativePath);

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath), cancellationToken);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public string GetImageUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return string.Empty;

        // Ensure path uses forward slashes
        return filePath.Replace("\\", "/").TrimStart('/');
    }

    public bool IsValidImageFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }

    public bool IsValidImageSize(long fileSize)
    {
        return fileSize > 0 && fileSize <= _maxFileSizeBytes;
    }
}
