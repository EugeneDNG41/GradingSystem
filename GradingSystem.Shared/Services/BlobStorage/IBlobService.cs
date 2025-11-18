namespace GradingSystem.Shared.Services.BlobStorage;
public interface IBlobService
{
    Task<Stream> DownloadAsync(string blobName, CancellationToken cancellationToken = default);
}
