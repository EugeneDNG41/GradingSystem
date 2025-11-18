using GradingSystem.Services.Exams.Api.Data;
using GradingSystem.Services.Exams.Api.Extensions;
using GradingSystem.Services.Exams.Api.Models;
using GradingSystem.Services.Exams.Api.Services;

namespace GradingSystem.Services.Exams.Api.Endpoints
{
    internal sealed class AssignExaminer : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/exams/assign-examiner",
                async (AssignExaminerRequest request, IExamExaminerService service) =>
                {
                    var entity = new ExamExaminer
                    {
                        ExamId = request.ExamId,
                        UserId = request.UserId
                    };

                    var result = await service.AsignExaminer(entity);

                    return result.Match(
                        Results.NoContent,
                        CustomResults.Problem
                    );
                })
            .WithTags("exams");
        }
    }
}
