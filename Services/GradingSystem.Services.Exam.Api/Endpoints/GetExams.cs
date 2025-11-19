using GradingSystem.Services.Exams.Api.Extensions;
using GradingSystem.Services.Exams.Api.Services;
using Microsoft.AspNetCore.Routing;

namespace GradingSystem.Services.Exams.Api.Endpoints;

public sealed class GetExams : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/exams", async (
            int? semesterId,
            IExamService examService
        ) =>
        {
            var result = await examService.GetExamListAsync(semesterId);
            return result.Match(Results.Ok, CustomResults.Problem);

        }).WithTags("exams");
    }
}
