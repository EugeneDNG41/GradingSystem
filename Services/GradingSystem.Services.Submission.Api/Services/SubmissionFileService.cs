using GradingSystem.Services.Submissions.Api.Data;
using GradingSystem.Services.Submissions.Api.Services.BlobStorage;
using GradingSystem.Shared;
using System.Linq;

namespace GradingSystem.Services.Submissions.Api.Services;

public class SubmissionFileService(
    IBlobService blobService,
    ISubmissionIngestionService ingestionService,
    ISubmissionValidationService validationService,
    ISubmissionAssetService assetService,
    ILogger<SubmissionFileService> logger) : ISubmissionFileService
{
    private readonly IBlobService _blobService = blobService;
    private readonly ISubmissionIngestionService _ingestionService = ingestionService;
    private readonly ISubmissionValidationService _validationService = validationService;
    private readonly ISubmissionAssetService _assetService = assetService;
    private readonly ILogger<SubmissionFileService> _logger = logger;

    public async Task<Result<UnpackResult>> UnpackAsync(
        SubmissionBatch submissionBatch,
        CancellationToken cancellationToken = default)
    {
        IngestionResult? ingestionResult = null;

        try
        {
            await using var archiveStream = await _blobService.DownloadAsync(
                submissionBatch.SubmissionFile.BlobName,
                cancellationToken);

            var ingestion = await _ingestionService.IngestAsync(archiveStream, cancellationToken);
            if (ingestion.IsFailure)
            {
                return Result.Failure<UnpackResult>(ingestion.Error);
            }

            ingestionResult = ingestion.Value;

            var validationOutcome = await _validationService.ValidateAsync(
                submissionBatch,
                ingestionResult.ExtractedFiles,
                cancellationToken);

            var assetResult = await _assetService.PersistAssetsAsync(
                submissionBatch,
                validationOutcome.Entries.Select(e => e.Id).ToList(),
                ingestionResult.ExtractedFiles,
                cancellationToken);

            if (assetResult.IsFailure)
            {
                return Result.Failure<UnpackResult>(assetResult.Error);
            }

            return Result.Success(new UnpackResult
            {
                IsSuccess = validationOutcome.Validation.IsValid,
                ExtractedFiles = ingestionResult.ExtractedFiles.ToList(),
                Validation = validationOutcome.Validation,
                ValidationOutcome = validationOutcome
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing submission batch {BatchId}.", submissionBatch.Id);
            return Result.Failure<UnpackResult>(Error.Failure(
                "Submissions.Process.Unexpected",
                "Failed to process submission archive."));
        }
        finally
        {
            CleanupTempDirectory(ingestionResult?.ExtractionPath);
        }
    }

    public async Task<Result<bool>> UploadFile(IFormFile file)
    {
        if (file.Length == 0)
        {
            return Result.Failure<bool>(Error.BadRequest("001", "Empty file"));
        }

        try
        {
            var uploadPath = Path.Combine(Path.GetTempPath(), file.FileName);
            await using var stream = new FileStream(uploadPath, FileMode.Create);
            await file.CopyToAsync(stream);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
            return Result.Failure<bool>(Error.Failure("002", "File upload failed"));
        }
    }

    private void CleanupTempDirectory(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cleaning temporary directory {Path}", path);
        }
    }
}
