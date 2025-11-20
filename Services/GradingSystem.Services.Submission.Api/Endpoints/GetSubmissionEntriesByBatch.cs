using GradingSystem.Services.Submissions.Api.Extensions;
using GradingSystem.Services.Submissions.Api.Services;

namespace GradingSystem.Services.Submissions.Api.Endpoints;

internal sealed class GetSubmissionEntriesByBatch : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/submissions/batches/{submissionBatchId:int}/entries", async (
            int submissionBatchId,
            ISubmissionBatchService service) =>
        {
            var result = await service.GetEntriesByBatchIdAsync(submissionBatchId);
            return result.Match(
                Results.Ok,
                CustomResults.Problem
            );
        })
        .RequireAuthorization()
        .WithTags("submissions");
    }
}

