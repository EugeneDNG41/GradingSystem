using GradingSystem.Services.Submissions.Api.Data;
using GradingSystem.Services.Submissions.Api.Models;
using GradingSystem.Shared;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Submissions.Api.Services;

public class SubmissionBatchService : ISubmissionBatchService
{
    private readonly SubmissionsDbContext _db;

    public SubmissionBatchService(SubmissionsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> AssignExaminerAsync(AssignExaminerToBatchRequest request)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var batch = await _db.SubmissionBatches
                .FirstOrDefaultAsync(b => b.Id == request.SubmissionBatchId);

            if (batch is null)
            {
                return Result.Failure<int>(Error.NotFound(
                    "BATCH404",
                    "Submission batch not found."
                ));
            }

            var existingBatchAssignment = await _db.AssignedExaminers
                .FirstOrDefaultAsync(a => a.SubmissionBatchId == request.SubmissionBatchId
                                       && a.ExaminerId == request.ExaminerId);

            if (existingBatchAssignment != null)
            {
                return existingBatchAssignment.Id;
            }

            var assignment = new AssignedExaminer
            {
                ExamId = batch.ExamId,
                ExaminerId = request.ExaminerId,
                SubmissionBatchId = request.SubmissionBatchId,
                AssignedAt = DateTime.UtcNow
            };

            _db.AssignedExaminers.Add(assignment);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
            return assignment.Id;
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            var existing = await _db.AssignedExaminers
                .FirstOrDefaultAsync(a => a.SubmissionBatchId == request.SubmissionBatchId
                                       && a.ExaminerId == request.ExaminerId);

            if (existing != null)
            {
                return existing.Id;
            }

            throw;
        }
    }

    public async Task<Result<List<SubmissionEntryResponse>>> GetEntriesByBatchIdAsync(int submissionBatchId)
    {
        var batchExists = await _db.SubmissionBatches
            .AnyAsync(b => b.Id == submissionBatchId);

        if (!batchExists)
        {
            return Result.Failure<List<SubmissionEntryResponse>>(Error.NotFound(
                "BATCH404",
                "Submission batch not found."
            ));
        }

        var entries = await _db.SubmissionEntries
            .Where(e => e.SubmissionBatchId == submissionBatchId)
            .OrderBy(e => e.StudentCode)
            .Select(e => new SubmissionEntryResponse(
                e.Id,
                e.SubmissionBatchId,
                e.StudentCode,
                e.TextHash,
                e.TemporaryScore,
                e.ExtractedAt
            ))
            .ToListAsync();

        return entries;
    }

    public async Task<Result<List<SubmissionBatchListItem>>> GetSubmissionBatchesAsync()
    {
        // Get all batches with exam titles
        var batchesQuery = from batch in _db.SubmissionBatches
                          join exam in _db.CachedExams on batch.ExamId equals exam.Id into examGroup
                          from exam in examGroup.DefaultIfEmpty()
                          orderby batch.Id descending
                          select new
                          {
                              batch.Id,
                              batch.Notes,
                              ExamTitle = exam != null ? exam.Title : null,
                              BatchId = batch.Id,
                              ExamId = batch.ExamId
                          };

        var batchesData = await batchesQuery.ToListAsync();

        // Get batch IDs
        var batchIds = batchesData.Select(b => b.BatchId).ToList();

        // Get examiner assignments for these batches (only take first examiner per batch if multiple exist)
        var assignments = await _db.AssignedExaminers
            .Where(ae => ae.SubmissionBatchId != null && batchIds.Contains(ae.SubmissionBatchId.Value))
            .OrderBy(a => a.AssignedAt)
            .ToListAsync();

        // Get examiner IDs
        var examinerIds = assignments.Select(a => a.ExaminerId).Distinct().ToList();

        // Get examiner names
        var examiners = await _db.CachedExaminers
            .Where(e => examinerIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.Name);

        // Create lookup for batch to examiner mapping (take first examiner per batch)
        var batchExaminerLookup = assignments
            .GroupBy(a => a.SubmissionBatchId!.Value)
            .ToDictionary(g => g.Key, g => g.First().ExaminerId);

        // Map to result
        var result = batchesData.Select(b => new SubmissionBatchListItem(
            b.Id,
            b.Notes,
            b.ExamTitle,
            batchExaminerLookup.TryGetValue(b.BatchId, out var examinerId) && examiners.TryGetValue(examinerId, out var examinerName)
                ? examinerName
                : null,
            b.ExamId
        )).ToList();

        return result;
    }

    public async Task<Result<SubmissionBatchListItem>> GetSubmissionBatchByIdAsync(int submissionBatchId)
    {
        // Get batch with exam title
        var batchData = await (from batch in _db.SubmissionBatches
                              where batch.Id == submissionBatchId
                              join exam in _db.CachedExams on batch.ExamId equals exam.Id into examGroup
                              from exam in examGroup.DefaultIfEmpty()
                              select new
                              {
                                  batch.Id,
                                  batch.Notes,
                                  ExamTitle = exam != null ? exam.Title : null,
                                  BatchId = batch.Id,
                                  ExamId = batch.ExamId
                              })
                              .FirstOrDefaultAsync();

        if (batchData == null)
        {
            return Result.Failure<SubmissionBatchListItem>(Error.NotFound(
                "BATCH404",
                "Submission batch not found."
            ));
        }

        // Get examiner assignment for this batch (only take first examiner if multiple exist)
        var assignment = await _db.AssignedExaminers
            .Where(ae => ae.SubmissionBatchId == submissionBatchId)
            .OrderBy(a => a.AssignedAt)
            .FirstOrDefaultAsync();

        string? examinerName = null;
        if (assignment != null)
        {
            var examiner = await _db.CachedExaminers
                .FirstOrDefaultAsync(e => e.Id == assignment.ExaminerId);
            
            examinerName = examiner?.Name;
        }

        var result = new SubmissionBatchListItem(
            batchData.Id,
            batchData.Notes,
            batchData.ExamTitle,
            examinerName,
            batchData.ExamId
        );

        return result;
    }

    public async Task<Result<List<SubmissionBatchListItem>>> GetBatchesByExaminerIdAsync(int examinerId)
    {
        // Get batches assigned to this examiner
        var batchIds = await _db.AssignedExaminers
            .Where(ae => ae.ExaminerId == examinerId && ae.SubmissionBatchId != null)
            .Select(ae => ae.SubmissionBatchId!.Value)
            .Distinct()
            .ToListAsync();

        if (batchIds.Count == 0)
        {
            return new List<SubmissionBatchListItem>();
        }

        // Get batches with exam titles
        var batchesQuery = from batch in _db.SubmissionBatches
                          where batchIds.Contains(batch.Id)
                          join exam in _db.CachedExams on batch.ExamId equals exam.Id into examGroup
                          from exam in examGroup.DefaultIfEmpty()
                          orderby batch.Id descending
                          select new
                          {
                              batch.Id,
                              batch.Notes,
                              ExamTitle = exam != null ? exam.Title : null,
                              BatchId = batch.Id,
                              ExamId = batch.ExamId
                          };

        var batchesData = await batchesQuery.ToListAsync();

        // Get examiner name
        var examiner = await _db.CachedExaminers
            .FirstOrDefaultAsync(e => e.Id == examinerId);

        var examinerName = examiner?.Name;

        // Map to result
        var result = batchesData.Select(b => new SubmissionBatchListItem(
            b.Id,
            b.Notes,
            b.ExamTitle,
            examinerName,
            b.ExamId
        )).ToList();

        return result;
    }
}

