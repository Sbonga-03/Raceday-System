using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services;

public interface IImageStorageService
{
    /// <summary>Uploads an image and returns the public URL of the stored file.</summary>
    Task<string> UploadImageAsync(IFormFile file, string folder);
}

public class BlobImageStorageService : IImageStorageService
{
    private static readonly string[] AllowedTypes = { "image/jpeg", "image/png", "image/webp" };
    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IConfiguration _config;

    public BlobImageStorageService(IConfiguration config) => _config = config;

    public async Task<string> UploadImageAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("No file was uploaded.");
        if (file.Length > MaxBytes)
            throw new InvalidOperationException("The image must be 5 MB or smaller.");
        if (!AllowedTypes.Contains(file.ContentType))
            throw new InvalidOperationException("Only JPG, PNG or WebP images are allowed.");

        var connectionString = _config["AzureStorage:ConnectionString"];
        var containerName = _config["AzureStorage:ContainerName"];
        if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(containerName))
            throw new InvalidOperationException("Image storage is not configured.");

        var container = new BlobContainerClient(connectionString, containerName);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var blobName = $"{folder}/{Guid.NewGuid():N}{extension}";
        var blob = container.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

        return blob.Uri.ToString();
    }
}