using GradingSystem.Services.Exams.Api.Services;
using GradingSystem.Services.Exams.Api.Extensions;

namespace GradingSystem.Services.Exams.Api.Endpoints;

public class DeleteRubric : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/rubrics/{id:int}", async (
            int id,
            IRubricService service
        ) =>
        {
            var result = await service.DeleteRubricAsync(id);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags("rubrics");
    }
}
