using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace GradingSystem.Services.Submissions.Api.Services.BlobStorage;
public class BlobService(
    BlobServiceClient blobServiceClient,
    ILogger<BlobService> logger) : IBlobService
{
    private const string DefaultContainer = "submissions";
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient;
    private readonly string _containerName = DefaultContainer;
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

    public async Task<string> UploadAsync(
        Stream content,
        string blobName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlobClient(blobName);
            content.Position = 0;

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            };

            await blobClient.UploadAsync(content, uploadOptions, cancellationToken);
            _logger.LogInformation("Uploaded blob {BlobName} to container {Container}", blobName, _containerName);

            return blobName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading blob {BlobName} to container {Container}", blobName, _containerName);
            throw;
        }
    }
}
