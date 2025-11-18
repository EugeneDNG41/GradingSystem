namespace GradingSystem.Services.Exams.Api.Models
{
    public record CreateSemesterRequest(
        string Name,
        DateTime StartDate,
        DateTime EndDate
    );  
}
