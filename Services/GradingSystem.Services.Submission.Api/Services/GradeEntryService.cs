using GradingSystem.Services.Submissions.Api.Data;
using GradingSystem.Services.Submissions.Api.Models;
using GradingSystem.Shared;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Submissions.Api.Services
{
    public class GradeEntryService : IGradeEntryService
    {
        private readonly SubmissionsDbContext _db;

        public GradeEntryService(SubmissionsDbContext db)
        {
            _db = db;
        }

        public async Task<Result<int>> CreateAsync(CreateGradeEntryRequest request)
        {
            var entry = await _db.SubmissionEntries
                .Include(e => e.SubmissionBatch)
                .FirstOrDefaultAsync(e => e.Id == request.SubmissionEntryId);

            if (entry is null)
            {
                return Result.Failure<int>(Error.NotFound(
                    "SUB404", "Submission entry not found."
                ));
            }

            var examId = entry.SubmissionBatch.ExamId;

            var existingGrade = await _db.GradeEntries
                .Where(x => x.SubmissionEntryId == request.SubmissionEntryId
                         && x.ExaminerId == request.ExaminerId)
                .FirstOrDefaultAsync();

            if (existingGrade is not null)
            {
                return Result.Failure<int>(Error.Conflict(
                    "GRD409",
                    "A grade entry by this examiner for the submission already exists."
                ));
            }

            bool allowed = await _db.AssignedExaminers
                .AnyAsync(x => x.ExamId == examId
                            && x.ExaminerId == request.ExaminerId);

            if (!allowed)
            {
                return Result.Failure<int>(
                    Error.Forbidden("NOT_ALLOWED", "You are not assigned to this exam")
                );
            }

            var grade = new GradeEntry
            {
                SubmissionEntryId = request.SubmissionEntryId,
                ExaminerId = request.ExaminerId,
                Score = request.Score,
                Notes = request.Notes,
                GradedAt = DateTime.UtcNow
            };

            _db.GradeEntries.Add(grade);

            entry.TemporaryScore = request.Score;

            await _db.SaveChangesAsync();

            return grade.Id;
        }


        public async Task<Result<List<GradeEntryResponse>>> GetBySubmissionAsync(int submissionEntryId)
        {
            var list = await _db.GradeEntries
                .Where(x => x.SubmissionEntryId == submissionEntryId)
                .OrderByDescending(x => x.GradedAt)
                .Select(x => new GradeEntryResponse(
                    x.Id,
                    x.SubmissionEntryId,
                    x.ExaminerId,
                    x.Score,
                    x.Notes,
                    x.GradedAt
                ))
                .ToListAsync();

            return Result.Success(list);
        }
        public async Task<Result<List<GradeEntryResponse>>> GetAllAsync(int? examinerId)
        {
            var query = _db.GradeEntries.AsQueryable();

            if (examinerId is not null)
            {
                var existingExaminer = await _db.CachedExaminers.FindAsync(examinerId.Value);
                if (existingExaminer is null)
                {
                    return Result.Failure<List<GradeEntryResponse>>(Error.NotFound(
                        "EXM404", "Examiner not found."
                    ));
                }
                query = query.Where(g => g.ExaminerId == examinerId);
            }

            var list = await query
                .OrderByDescending(g => g.GradedAt)
                .Select(g => new GradeEntryResponse(
                    g.Id,
                    g.SubmissionEntryId,
                    g.ExaminerId,
                    g.Score,
                    g.Notes,
                    g.GradedAt
                ))
                .ToListAsync();

            return Result.Success(list);
        }

    }

}
