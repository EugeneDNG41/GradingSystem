using GradingSystem.Services.Exams.Api.Extensions;
using GradingSystem.Services.Exams.Api.Models;
using GradingSystem.Services.Exams.Api.Services;

namespace GradingSystem.Services.Exams.Api.Endpoints
{
    public class UpdateRubric
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/rubrics/{id:int}", async (
                int id,
                UpdateRubricRequest request,
                IRubricService service
            ) =>
            {
                var result = await service.UpdateRubricAsync(id, request);
                return result.Match(Results.NoContent, CustomResults.Problem);
            })
            .WithTags("rubrics");
        }
    }
}
