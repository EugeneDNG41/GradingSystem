using GradingSystem.Services.Submissions.Api.Extensions;
using GradingSystem.Services.Submissions.Api.Services;

namespace GradingSystem.Services.Submissions.Api.Endpoints;

internal sealed class GetSubmissionBatches : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/submissions/batches", async (ISubmissionBatchService service) =>
        {
            var result = await service.GetSubmissionBatchesAsync();
            return result.Match(
                Results.Ok,
                CustomResults.Problem
            );
        })
        .RequireAuthorization()
        .WithTags("submissions");
    }
}

