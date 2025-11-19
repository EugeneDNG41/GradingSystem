using GradingSystem.Services.Submissions.Api.Data;
using GradingSystem.Services.Submissions.Api.Models;
using GradingSystem.Services.Submissions.Api.Services.BlobStorage;
using GradingSystem.Shared;
using GradingSystem.Shared.Contracts;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace GradingSystem.Services.Submissions.Api.Services;

public sealed class SubmissionUploadService(
    IBlobService blobService,
    SubmissionsDbContext dbContext,
    ISubmissionFileService submissionFileService,
    ILogger<SubmissionUploadService> logger,
    IMessageBus messageBus) : ISubmissionUploadService
{
    private static readonly string[] AllowedExtensions = [".rar"];
    private readonly IBlobService _blobService = blobService;
    private readonly SubmissionsDbContext _dbContext = dbContext;
    private readonly ISubmissionFileService _submissionFileService = submissionFileService;
    private readonly ILogger<SubmissionUploadService> _logger = logger;
    private readonly IMessageBus _messageBus = messageBus;

    public async Task<Result<SubmissionUploadResponse>> UploadAsync(
        UploadSubmissionRequest request,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = ValidateRequest(request, userId);
            if (validationResult.IsFailure)
            {
                return Result.Failure<SubmissionUploadResponse>(validationResult.Error);
            }

            // var examExists = await _dbContext.CachedExams
            //     .AsNoTracking()
            //     .AnyAsync(e => e.Id == request.ExamId, cancellationToken);
            // if (!examExists)
            // {
            //     return Result.Failure<SubmissionUploadResponse>(Error.NotFound(
            //         "Submissions.Upload.ExamNotFound",
            //         "Exam information is not available. Please try again later."));
            // }

            var fileExtension = Path.GetExtension(request.Archive!.FileName);
            var blobName = $"{Guid.NewGuid():N}{fileExtension}";

            await using var archiveStream = request.Archive.OpenReadStream();
            var contentType = string.IsNullOrWhiteSpace(request.Archive.ContentType)
                ? "application/vnd.rar"
                : request.Archive.ContentType;

            await _blobService.UploadAsync(archiveStream, blobName, contentType, cancellationToken);

            var submissionFile = new SubmissionFile
            {
                BlobName = blobName,
                UploadDate = DateTime.UtcNow
            };

            var submissionBatch = new SubmissionBatch
            {
                SubmissionFile = submissionFile,
                ExamId = request.ExamId,
                UploadedByUserId = userId,
                Status = SubmissionBatchStatus.Processing,
                UploadedAt = DateTime.UtcNow,
                Notes = request.Notes
            };

            _dbContext.SubmissionBatches.Add(submissionBatch);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var unpackResult = await _submissionFileService.UnpackAsync(submissionBatch, cancellationToken);
            if (unpackResult.IsFailure)
            {
                submissionBatch.Status = SubmissionBatchStatus.Failed;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Result.Failure<SubmissionUploadResponse>(unpackResult.Error);
            }

            submissionBatch.Status = unpackResult.Value.Validation.IsValid
                ? SubmissionBatchStatus.Completed
                : SubmissionBatchStatus.Failed;

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (unpackResult.Value.ValidationOutcome?.Violations.Count > 0)
            {
                try
                {
                    await PublishViolationsEventAsync(
                        submissionBatch,
                        unpackResult.Value.ValidationOutcome!,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Không thể phát sự kiện vi phạm cho batch {BatchId}", submissionBatch.Id);
                }
            }

            return Result.Success(new SubmissionUploadResponse
            {
                Id = submissionBatch.Id,
                Status = submissionBatch.Status,
                UploadedAt = submissionBatch.UploadedAt,
                ExamId = submissionBatch.ExamId,
                UploadedByUserId = submissionBatch.UploadedByUserId,
                Notes = submissionBatch.Notes,
                Validation = unpackResult.Value.Validation,
                File = new SubmissionUploadFileResponse
                {
                    Id = submissionFile.Id,
                    BlobName = submissionFile.BlobName,
                    UploadDate = submissionFile.UploadDate
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload failed");
            return Result.Failure<SubmissionUploadResponse>(Error.BadRequest("101", "Upload thất bại, vui lòng thử lại."));
        }
    }

    private static Result ValidateRequest(UploadSubmissionRequest request, int userId)
    {
        if (request.Archive is null || request.Archive.Length == 0)
        {
            return Result.Failure(Error.BadRequest(
                "Submissions.Upload.EmptyArchive",
                "Archive file is required."));
        }

        if (request.ExamId <= 0)
        {
            return Result.Failure(Error.BadRequest(
                "Submissions.Upload.InvalidExamId",
                "ExamId must be greater than zero."));
        }

        if (userId <= 0)
        {
            return Result.Failure(Error.Unauthorized(
                "Submissions.Upload.InvalidUser",
                "Authenticated user identifier is invalid."));
        }

        var fileExtension = Path.GetExtension(request.Archive.FileName);
        if (!AllowedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure(Error.BadRequest(
                "Submissions.Upload.UnsupportedExtension",
                "Only .rar archives are supported."));
        }

        return Result.Success();
    }

    private async Task PublishViolationsEventAsync(
        SubmissionBatch batch,
        ValidationOutcome validationOutcome,
        CancellationToken cancellationToken)
    {
        var students = validationOutcome.Entries
            .Select(entry =>
            {
                var entryViolations = validationOutcome.Violations
                    .Where(v => v.SubmissionEntryId == entry.Id)
                    .Select(v => new SubmissionViolationEnvelope(
                        v.Id,
                        v.Type.ToString(),
                        v.Severity,
                        v.Description))
                    .ToList();

                if (entryViolations.Count == 0)
                {
                    return null;
                }

                var metadata = SubmissionEntryMetadataSerializer.Deserialize(entry.Metadata) ?? new SubmissionEntryMetadata
                {
                    StudentCode = entry.StudentCode
                };

                return new StudentViolationEnvelope(
                    entry.Id,
                    entry.StudentCode,
                    entryViolations,
                    new SubmissionEntryMetadataEnvelope(
                        metadata.StudentCode,
                        metadata.TotalFiles,
                        metadata.TotalSize,
                        metadata.Files.Select(f => new SubmissionEntryFileMetadataEnvelope(
                            f.RelativePath,
                            f.Size,
                            f.Extension,
                            f.Category.ToString())).ToList(),
                        metadata.MissingItems,
                        metadata.ExtraItems));
            })
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

        if (students.Count == 0)
        {
            return;
        }

        var message = new SubmissionViolationsDetected(
            batch.Id,
            batch.ExamId,
            batch.UploadedByUserId,
            DateTime.UtcNow,
            students);

        await _messageBus.PublishAsync(message);
    }
}


