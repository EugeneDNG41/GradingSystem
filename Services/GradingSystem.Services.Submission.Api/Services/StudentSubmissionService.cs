using GradingSystem.Services.Submissions.Api.Data;
using GradingSystem.Services.Submissions.Api.Models;
using GradingSystem.Shared;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Submissions.Api.Services;

public sealed class StudentSubmissionService : IStudentSubmissionService
{
    private readonly SubmissionsDbContext _dbContext;

    public StudentSubmissionService(SubmissionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<StudentSubmissionResponse>>> GetByStudentCodeAsync(string studentCode)
    {
        if (string.IsNullOrWhiteSpace(studentCode))
        {
            return Result.Failure<List<StudentSubmissionResponse>>(
                Error.BadRequest("STU001", "Student code cannot be empty."));
        }

        var entries = await _dbContext.SubmissionEntries
            .Include(e => e.SubmissionBatch)
            .Include(e => e.Assets)
            .Include(e => e.Violations)
            .Where(e => e.StudentCode == studentCode)
            .OrderByDescending(e => e.ExtractedAt)
            .ToListAsync();

        var gradeEntries = await _dbContext.GradeEntries
            .Where(g => entries.Select(e => e.Id).Contains(g.SubmissionEntryId))
            .ToListAsync();

        var response = entries.Select(entry =>
        {
            var entryGradeEntries = gradeEntries
                .Where(g => g.SubmissionEntryId == entry.Id)
                .OrderByDescending(g => g.GradedAt)
                .Select(g => new GradeEntryResponse(
                    g.Id,
                    g.SubmissionEntryId,
                    g.ExaminerId,
                    g.Score,
                    g.Notes,
                    g.GradedAt
                ))
                .ToList();

            return new StudentSubmissionResponse(
                entry.Id,
                entry.SubmissionBatchId,
                entry.StudentCode,
                entry.Metadata,
                entry.TemporaryScore,
                entry.ExtractedAt,
                entry.SubmissionBatch.ExamId,
                entry.SubmissionBatch.UploadedAt,
                entry.Assets.Select(a => new StudentAssetResponse(
                    a.Id,
                    a.FileName,
                    a.MimeType,
                    a.BlobName,
                    a.CreatedAt
                )).ToList(),
                entry.Violations.Select(v => new StudentViolationResponse(
                    v.Id,
                    v.Type.ToString(),
                    v.Severity,
                    v.Description,
                    v.AssetLink,
                    v.SubmissionAssetId
                )).ToList(),
                entryGradeEntries
            );
        }).ToList();

        return Result.Success(response);
    }
}

