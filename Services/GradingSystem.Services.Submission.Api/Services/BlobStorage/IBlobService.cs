namespace GradingSystem.Services.Submissions.Api.Services.BlobStorage;
public interface IBlobService
{
    Task<Stream> DownloadAsync(string blobName, CancellationToken cancellationToken = default);
    Task<string> UploadAsync(
            Stream content,
            string blobName,
            string contentType,
            CancellationToken cancellationToken = default);
}
