using GradingSystem.Services.Submissions.Api.Extensions;
using GradingSystem.Services.Submissions.Api.Services;

namespace GradingSystem.Services.Submissions.Api.Endpoints
{
    internal sealed class GetAllGradeEntries : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/grading/entries", async (
                int? examinerId,                
                IGradeEntryService service) =>
            {
                var result = await service.GetAllAsync(examinerId);

                return result.Match(
                    Results.Ok,
                    CustomResults.Problem
                );
            })
            .WithTags("grading");
        }
    }

}
