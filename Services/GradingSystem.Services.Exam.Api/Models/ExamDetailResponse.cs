namespace GradingSystem.Services.Exams.Api.Models
{
    public record ExaminerInfo(int Id, string Name);

    public record RubricInfo(int Id, string Criteria, decimal MaxScore, int OrderIndex);

    public record SemesterInfo(int Id, string Name, DateTime StartDate, DateTime EndDate);

    public record ExamDetailResponse(
        int Id,
        string Title,
        DateTime DueDate,
        SemesterInfo Semester,
        IReadOnlyList<RubricInfo> Rubrics,
        IReadOnlyList<ExaminerInfo> Examiners
    );
}
