using GradingSystem.Services.Exams.Api.Extensions;
using GradingSystem.Services.Exams.Api.Models;
using GradingSystem.Services.Exams.Api.Services;
using Microsoft.AspNetCore.Routing;

namespace GradingSystem.Services.Exams.Api.Endpoints
{
    internal sealed class CreateExam : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/exams", async (CreateExamRequest request, IExamService service) =>
            {
                var result = await service.CreateExamAsync(request);

                return result.Match(
                    Results.Ok,
                    CustomResults.Problem
                );
            })
            .WithTags("exams");
        }
    }
}
