using System.IO;
using System.Linq;
using GradingSystem.Services.Submissions.Api.Data;
using GradingSystem.Services.Submissions.Api.Models;
using GradingSystem.Shared.Services.BlobStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GradingSystem.Services.Submissions.Api.Endpoints;

internal sealed class UploadFiles : IEndpoint
{
    private static readonly string[] AllowedExtensions = [".rar"];

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/submissions/upload", async (
            [FromForm] UploadSubmissionRequest request,
            IBlobService blobService,
            SubmissionsDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            if (request.Archive is null || request.Archive.Length == 0)
            {
                return Results.BadRequest(new { error = "Archive file is required." });
            }

            if (request.ExamId <= 0)
            {
                return Results.BadRequest(new { error = "ExamId must be greater than zero." });
            }

            if (request.UploadedByUserId <= 0)
            {
                return Results.BadRequest(new { error = "UploadedByUserId must be greater than zero." });
            }

            var fileExtension = Path.GetExtension(request.Archive.FileName);
            if (!AllowedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
            {
                return Results.BadRequest(new { error = "Only .rar archives are supported." });
            }

            var blobName = $"submissions/{request.ExamId}/{Guid.NewGuid():N}{fileExtension}";
            await using var archiveStream = request.Archive.OpenReadStream();

            var contentType = string.IsNullOrWhiteSpace(request.Archive.ContentType)
                ? "application/vnd.rar"
                : request.Archive.ContentType;

            await blobService.UploadAsync(archiveStream, blobName, contentType, cancellationToken);

            var submissionFile = new SubmissionFile
            {
                BlobName = blobName,
                UploadDate = DateTime.UtcNow
            };

            var submissionBatch = new SubmissionBatch
            {
                SubmissionFile = submissionFile,
                ExamId = request.ExamId,
                UploadedByUserId = request.UploadedByUserId,
                Status = SubmissionBatchStatus.PendingExtraction,
                UploadedAt = DateTime.UtcNow,
                Notes = request.Notes
            };

            dbContext.SubmissionBatches.Add(submissionBatch);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Created($"/submissions/batches/{submissionBatch.Id}", new
            {
                submissionBatch.Id,
                submissionBatch.Status,
                submissionBatch.UploadedAt,
                submissionBatch.ExamId,
                submissionBatch.UploadedByUserId,
                submissionBatch.Notes,
                File = new
                {
                    submissionFile.Id,
                    submissionFile.BlobName,
                    submissionFile.UploadDate
                }
            });
        })
        .Accepts<UploadSubmissionRequest>("multipart/form-data")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .WithTags("submissions");
    }
}
