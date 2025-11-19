using GradingSystem.Services.Submissions.Api.Extensions;
using GradingSystem.Services.Submissions.Api.Services;

namespace GradingSystem.Services.Submissions.Api.Endpoints;

internal sealed class SubmissionSolutionEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/submissions/{submissionEntryId:int}/solution/files", async (
            int submissionEntryId,
            ISubmissionSolutionService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ListFilesAsync(submissionEntryId, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("submissions")
        .RequireAuthorization();

        app.MapGet("/submissions/{submissionEntryId:int}/solution/files/{**filePath}", async (
            int submissionEntryId,
            string filePath,
            ISubmissionSolutionService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.DownloadSolutionFileAsync(submissionEntryId, filePath, cancellationToken);
            return result.Match(
                download => TypedResults.File(download.Content, download.ContentType, download.FileName),
                CustomResults.Problem);
        })
        .WithTags("submissions")
        .RequireAuthorization();

        app.MapGet("/submissions/{submissionEntryId:int}/attachments", async (
            int submissionEntryId,
            ISubmissionSolutionService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ListAttachmentsAsync(submissionEntryId, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags("submissions")
        .RequireAuthorization();

        app.MapGet("/submissions/{submissionEntryId:int}/attachments/{attachmentAssetId:int}", async (
            int submissionEntryId,
            int attachmentAssetId,
            ISubmissionSolutionService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.DownloadAttachmentAsync(submissionEntryId, attachmentAssetId, cancellationToken);
            return result.Match(
                download => TypedResults.File(download.Content, download.ContentType, download.FileName),
                CustomResults.Problem);
        })
        .WithTags("submissions")
        .RequireAuthorization();
    }
}

