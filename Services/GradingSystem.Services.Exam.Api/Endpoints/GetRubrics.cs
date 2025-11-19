using GradingSystem.Services.Exams.Api.Extensions;
using GradingSystem.Services.Exams.Api.Services;
using Microsoft.AspNetCore.Routing;

namespace GradingSystem.Services.Exams.Api.Endpoints;

internal sealed class GetRubrics : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/rubrics", async (
            int? examId,
            IRubricService service
        ) =>
        {
            var result = await service.GetRubricsAsync(examId);
            return result.Match(Results.Ok, CustomResults.Problem);
        }).WithTags("rubrics");
    }
}

