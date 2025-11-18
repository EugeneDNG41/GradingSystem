namespace GradingSystem.Services.Exams.Api.Models
{
    public record CreateRubricRequest(
       string Criteria,
       decimal MaxScore,
       int OrderIndex,
       int ExamId
   );
}
