namespace GradingSystem.Services.Submissions.Api.Models;

public sealed record UpdateGradeEntryRequest(
    decimal Score,
    string? Notes
);

