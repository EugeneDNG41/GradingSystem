namespace GradingSystem.Services.Submissions.Api.Models
{
    public sealed record CreateGradeEntryRequest(
        int SubmissionEntryId,
        int ExaminerId,
        decimal Score,
        string? Notes
    );

}
