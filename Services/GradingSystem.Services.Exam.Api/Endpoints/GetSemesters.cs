using GradingSystem.Services.Exams.Api.Extensions;
using GradingSystem.Services.Exams.Api.Services;

namespace GradingSystem.Services.Exams.Api.Endpoints
{
    internal sealed class GetSemesters : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/semesters", async (ISemesterService service) =>
            {
                var result = await service.GetAllSemestersAsync();

                return result.Match(
                    Results.Ok,
                    CustomResults.Problem
                );
            })
            .WithTags("semesters");
        }
    }

}
