using GradingSystem.Services.Submissions.Api.Extensions;
using GradingSystem.Services.Submissions.Api.Services;

namespace GradingSystem.Services.Submissions.Api.Endpoints
{
    internal sealed class GetGradeEntriesBySubmission : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/grading/submissions/{submissionEntryId:int}", async (
                int submissionEntryId,
                IGradeEntryService service) =>
            {
                var result = await service.GetBySubmissionAsync(submissionEntryId);

                return result.Match(
                    Results.Ok,
                    CustomResults.Problem
                );
            })
            .RequireAuthorization()
            .WithTags("grading");
        }
    }

}
