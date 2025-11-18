using GradingSystem.Services.Exams.Api.Extensions;
using GradingSystem.Services.Exams.Api.Models;
using GradingSystem.Services.Exams.Api.Services;
using Microsoft.AspNetCore.Routing;

namespace GradingSystem.Services.Exams.Api.Endpoints
{
    internal sealed class CreateRubric : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/rubrics", async (CreateRubricRequest request, IRubricService service) =>
            {
                var result = await service.CreateRubricAsync(request);

                return result.Match(
                    Results.Ok,
                    CustomResults.Problem
                );
            })
            .WithTags("rubrics");
        }
    }
}
