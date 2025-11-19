namespace GradingSystem.Services.Exams.Api.Models
{
    public sealed record UpdateSemesterRequest(
       string Name,
       DateTime StartDate,
       DateTime EndDate
   );
}
