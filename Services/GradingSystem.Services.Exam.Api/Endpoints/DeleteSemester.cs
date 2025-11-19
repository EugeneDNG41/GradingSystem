using GradingSystem.Services.Exams.Api.Extensions;
using GradingSystem.Services.Exams.Api.Services;

namespace GradingSystem.Services.Exams.Api.Endpoints
{
    internal sealed class DeleteSemester : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/semesters/{id:int}", async (int id, ISemesterService service) =>
            {
                var result = await service.DeleteSemesterAsync(id);

                return result.Match(
                    _ => Results.NoContent(),
                    CustomResults.Problem
                );
            })
            .WithTags("semesters");
        }
    }

}
