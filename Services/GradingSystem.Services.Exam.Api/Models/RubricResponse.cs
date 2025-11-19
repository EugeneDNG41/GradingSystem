namespace GradingSystem.Services.Exams.Api.Models;

public record RubricResponse(
    int Id,
    string Criteria,
    decimal MaxScore,
    int OrderIndex,
    int ExamId
);

