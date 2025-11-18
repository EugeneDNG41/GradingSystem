namespace GradingSystem.Services.Exams.Api.Models
{
    public record CreateExamRequest(
        string Title,
        DateTime DueDate,
        int SemesterId
    );
}
