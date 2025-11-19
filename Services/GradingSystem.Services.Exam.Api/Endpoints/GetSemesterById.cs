using GradingSystem.Services.Exams.Api.Extensions;
using GradingSystem.Services.Exams.Api.Services;

namespace GradingSystem.Services.Exams.Api.Endpoints
{
    internal sealed class GetSemesterById : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/semesters/{id:int}", async (int id, ISemesterService service) =>
            {
                var result = await service.GetSemesterByIdAsync(id);

                return result.Match(
                    Results.Ok,
                    CustomResults.Problem
                );
            })
            .WithTags("semesters");
        }
    }

}
