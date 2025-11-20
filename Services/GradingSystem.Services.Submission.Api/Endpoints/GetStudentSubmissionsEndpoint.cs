using GradingSystem.Services.Submissions.Api.Extensions;
using GradingSystem.Services.Submissions.Api.Models;
using GradingSystem.Services.Submissions.Api.Services;

namespace GradingSystem.Services.Submissions.Api.Endpoints;

internal sealed class GetStudentSubmissionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/students/{studentCode}/submissions", async (
            string studentCode,
            IStudentSubmissionService service) =>
        {
            var result = await service.GetByStudentCodeAsync(studentCode);

            return result.Match(
                Results.Ok,
                CustomResults.Problem
            );
        })
        .WithTags("students")
        .Produces<List<StudentSubmissionResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}

