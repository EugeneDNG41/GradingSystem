namespace GradingSystem.Services.Exams.Api.Models
{
    public record UpdateRubricRequest(
        string Criteria,
        decimal MaxScore,
        int OrderIndex
    );
}
