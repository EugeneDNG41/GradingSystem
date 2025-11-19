namespace GradingSystem.Services.Submissions.Api.Models
{
    public sealed record GradeEntryFilterRequest(
        int? ExaminerId
    );
}
