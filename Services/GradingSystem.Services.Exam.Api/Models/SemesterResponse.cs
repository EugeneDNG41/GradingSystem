namespace GradingSystem.Services.Exams.Api.Models
{
    public record SemesterResponse(
        int Id,
        string Name,
        DateTime StartDate,
        DateTime EndDate
    );
}
