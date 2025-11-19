using Microsoft.AspNetCore.Http;

namespace GradingSystem.Services.Submissions.Api.Models;

public class UploadSubmissionRequest
{
    public IFormFile? Archive { get; set; }
    public int ExamId { get; set; }
    public int UploadedByUserId { get; set; }
    public string? Notes { get; set; }
}

