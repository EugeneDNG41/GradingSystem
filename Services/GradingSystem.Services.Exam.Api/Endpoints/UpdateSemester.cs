using GradingSystem.Services.Exams.Api.Extensions;
using GradingSystem.Services.Exams.Api.Models;
using GradingSystem.Services.Exams.Api.Services;

namespace GradingSystem.Services.Exams.Api.Endpoints
{
    internal sealed class UpdateSemester : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/semesters/{id:int}", async (int id, UpdateSemesterRequest request, ISemesterService service) =>
            {
                var result = await service.UpdateSemesterAsync(id, request);

                return result.Match(
                    Results.Ok,
                    CustomResults.Problem
                );
            })
            .WithTags("semesters");
        }
    }

}
