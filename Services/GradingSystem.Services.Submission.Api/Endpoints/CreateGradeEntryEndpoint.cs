using GradingSystem.Services.Submissions.Api.Extensions;
using GradingSystem.Services.Submissions.Api.Models;
using GradingSystem.Services.Submissions.Api.Services;

namespace GradingSystem.Services.Submissions.Api.Endpoints
{
    internal sealed class CreateGradeEntryEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/grading/entries", async (
                CreateGradeEntryRequest request,
                IGradeEntryService service) =>
            {
                var result = await service.CreateAsync(request);

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
