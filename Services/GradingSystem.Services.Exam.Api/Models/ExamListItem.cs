namespace GradingSystem.Services.Exams.Api.Models
{
    public record ExamListItem(
        int Id,
        string Title,
        DateTime DueDate,
        int SemesterId,
        string SemesterName,
        int ExaminerCount,
        int RubricCount
    );
}
