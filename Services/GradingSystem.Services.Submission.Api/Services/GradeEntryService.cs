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
            var entry = await _db.SubmissionEntries.FindAsync(request.SubmissionEntryId);

            if (entry is null)
                return Result.Failure<int>(Error.NotFound(
                    "SUB404", "Submission entry not found."
                ));

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
    }

}
