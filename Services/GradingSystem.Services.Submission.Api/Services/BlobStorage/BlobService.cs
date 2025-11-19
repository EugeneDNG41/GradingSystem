using System.Net.Http;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

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

        // Small helper local func for exponential backoff delay
        static Task BackoffDelayAsync(int attempt, CancellationToken ct) => Task.Delay(TimeSpan.FromSeconds(Math.Min(2 * attempt, 8)), ct);

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            // Create container with a small retry loop to tolerate transient HTTP timeouts
            const int maxCreateAttempts = 3;
            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
                    break;
                }
                catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    // Underlying HttpClient/transport likely timed out. Treat as transient.
                    _logger.LogWarning(ex, "CreateIfNotExistsAsync was canceled by underlying transport (attempt {Attempt}). Will retry if attempts remain.", attempt);
                    if (attempt >= maxCreateAttempts)
                    {
                        throw new HttpRequestException("Timeout while creating blob container after multiple attempts.", ex);
                    }

                    // small backoff, do not pass the request cancellation token here so we retry even if original request token is still valid
                    await BackoffDelayAsync(attempt, CancellationToken.None);
                    continue;
                }
            }

            var blobClient = containerClient.GetBlobClient(blobName);

            // Ensure we can retry the upload: stream must be seekable to reset position between attempts
            bool canRetryUpload = content.CanSeek;

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            };

            const int maxUploadAttempts = 3;
            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    if (canRetryUpload)
                    {
                        content.Position = 0;
                    }
                    else if (attempt > 1)
                    {
                        // Cannot retry non-seekable stream
                        _logger.LogError("Attempted to retry upload but provided stream is not seekable. Aborting.");
                        throw new InvalidOperationException("Cannot retry upload because the provided stream is not seekable.");
                    }

                    await blobClient.UploadAsync(content, uploadOptions, cancellationToken);
                    _logger.LogInformation("Uploaded blob {BlobName} to container {Container}", blobName, _containerName);
                    return blobName;
                }
                catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    // Underlying HttpClient timed out (or Azure SDK timed out) — treat as transient
                    _logger.LogWarning(ex, "UploadAsync was canceled by underlying transport (attempt {Attempt}). Will retry if attempts remain.", attempt);

                    if (attempt >= maxUploadAttempts)
                    {
                        // Exhausted retries — wrap to give clearer context
                        throw new HttpRequestException("Timeout while uploading blob after multiple attempts.", ex);
                    }

                    // Backoff before retry. Use CancellationToken.None so this backoff isn't cut short by the request token; if the request is cancelled the next call will observe it and abort quickly.
                    await BackoffDelayAsync(attempt, CancellationToken.None);
                    continue;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // The request cancellation token was signaled (client disconnected or request aborted). Respect it and rethrow so upstream can handle it as canceled request.
                    _logger.LogInformation("Upload canceled by request for blob {BlobName}.", blobName);
                    throw;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Ensure we log request cancellations at info level and propagate
            _logger.LogInformation("Upload canceled by request for blob {BlobName} in container {Container}.", blobName, _containerName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading blob {BlobName} to container {Container}", blobName, _containerName);
            throw;
        }
    }
}
