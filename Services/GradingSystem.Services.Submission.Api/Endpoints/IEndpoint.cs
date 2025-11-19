using GradingSystem.Services.Submissions.Api.Extensions;
using GradingSystem.Services.Submissions.Api.Models;
using GradingSystem.Services.Submissions.Api.Services;
using GradingSystem.Services.Submissions.Api.Services.BlobStorage;

namespace GradingSystem.Services.Submissions.Api.Endpoints;

internal interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
internal sealed class CreateSubmissionFile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/submissions", async (CreateSubmissionFileRequest request, ISubmissionFileService submissionService, IBlobService blobService) =>
        {
            var result = await submissionService.UnpackAsync(request.BlobName, blobService);
            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags("submissions");
    }
}