using GradingSystem.Services.Exams.Api.Extensions;
using GradingSystem.Services.Exams.Api.Models;
using GradingSystem.Services.Exams.Api.Services;
using Microsoft.AspNetCore.Routing;

namespace GradingSystem.Services.Exams.Api.Endpoints
{
    internal sealed class CreateSemester : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/semesters", async (CreateSemesterRequest request, ISemesterService service) =>
            {
                var result = await service.CreateSemesterAsync(request);

                return result.Match(
                    Results.Ok,
                    CustomResults.Problem
                );
            })
            .WithTags("semesters");
        }
    }
}
