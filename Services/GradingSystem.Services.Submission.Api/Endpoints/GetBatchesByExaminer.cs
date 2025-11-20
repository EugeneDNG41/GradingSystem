using GradingSystem.Services.Submissions.Api.Extensions;
using GradingSystem.Services.Submissions.Api.Services;

namespace GradingSystem.Services.Submissions.Api.Endpoints;

internal sealed class GetBatchesByExaminer : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/submissions/batches/by-examiner/{examinerId:int}", async (
            int examinerId,
            ISubmissionBatchService service) =>
        {
            var result = await service.GetBatchesByExaminerIdAsync(examinerId);
            return result.Match(
                Results.Ok,
                CustomResults.Problem
            );
        })
        .RequireAuthorization()
        .WithTags("submissions");
    }
}

