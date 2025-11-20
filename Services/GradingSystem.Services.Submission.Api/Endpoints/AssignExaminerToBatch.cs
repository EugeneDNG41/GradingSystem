using GradingSystem.Services.Submissions.Api.Extensions;
using GradingSystem.Services.Submissions.Api.Models;
using GradingSystem.Services.Submissions.Api.Services;

namespace GradingSystem.Services.Submissions.Api.Endpoints;

internal sealed class AssignExaminerToBatch : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/submissions/batches/assign-examiner", async (
            AssignExaminerToBatchRequest request,
            ISubmissionBatchService service) =>
        {
            var result = await service.AssignExaminerAsync(request);
            return result.Match(
                Results.Ok,
                CustomResults.Problem
            );
        })
        .RequireAuthorization()
        .WithTags("submissions");
    }
}

