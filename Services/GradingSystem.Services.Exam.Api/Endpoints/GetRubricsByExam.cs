using GradingSystem.Services.Exams.Api.Extensions;
using GradingSystem.Services.Exams.Api.Models;
using GradingSystem.Services.Exams.Api.Services;
using Microsoft.AspNetCore.Routing;

namespace GradingSystem.Services.Exams.Api.Endpoints
{
    internal sealed class GetRubricsByExam : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/exams/{examId:int}/rubrics", async (int examId, IRubricService service) =>
            {
                var result = await service.GetRubricsByExamIdAsync(examId);

                return result.Match(
                    Results.Ok,
                    CustomResults.Problem
                );
            })
            .WithTags("rubrics");
        }
    }
}
