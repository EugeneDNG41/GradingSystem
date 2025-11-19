using GradingSystem.Services.Exams.Api.Extensions;
using GradingSystem.Services.Exams.Api.Services;
using Microsoft.AspNetCore.Routing;

namespace GradingSystem.Services.Exams.Api.Endpoints
{
    internal sealed class GetExamById : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/exams/{id:int}", async (int id, IExamService service) =>
            {
                var result = await service.GetExamByIdAsync(id);

                return result.Match(
                    Results.Ok,
                    CustomResults.Problem
                );
            })
            .WithTags("exams");
        }
    }
}
