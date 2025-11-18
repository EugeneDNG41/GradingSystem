using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace GradingSystem.Shared.Services.BlobStorage;
public class BlobService(
    BlobServiceClient blobServiceClient,
    ILogger<BlobService> logger) : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient;
    private readonly string _containerName;
    private readonly ILogger<BlobService> _logger = logger;

    public async Task<Stream> DownloadAsync(string blobName, CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                throw new FileNotFoundException($"Blob not found: {blobName}");
            }

            var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            _logger.LogInformation("File downloaded successfully: {BlobName}", blobName);
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from blob storage: {BlobName}", blobName);
            throw;
        }
    }
}
