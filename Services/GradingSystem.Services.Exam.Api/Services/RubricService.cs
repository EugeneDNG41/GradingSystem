using GradingSystem.Services.Exams.Api.Data;
using GradingSystem.Services.Exams.Api.Models;
using GradingSystem.Shared;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Exams.Api.Services
{
    public class RubricService : IRubricService
    {
        private readonly ExamsDbContext _db;

        public RubricService(ExamsDbContext db)
        {
            _db = db;
        }

        public async Task<Result<int>> CreateRubricAsync(CreateRubricRequest request)
        {
            bool examExists = await _db.Exams.AnyAsync(x => x.Id == request.ExamId);
            if (!examExists)
            {
                return Result.Failure<int>(
                    Error.NotFound("EXAM40401", "Exam not found.")
                );
            }

            var rubric = new Rubric
            {
                Criteria = request.Criteria,
                MaxScore = request.MaxScore,
                OrderIndex = request.OrderIndex,
                ExamId = request.ExamId
            };

            _db.Rubrics.Add(rubric);
            await _db.SaveChangesAsync();

            return rubric.Id;
        }
        public async Task<Result> UpdateRubricAsync(int id, UpdateRubricRequest request)
        {
            var rubric = await _db.Rubrics.FindAsync(id);

            if (rubric == null)
                return Result.Failure(Error.NotFound("RUB404", "Rubric not found."));

            rubric.Criteria = request.Criteria;
            rubric.MaxScore = request.MaxScore;
            rubric.OrderIndex = request.OrderIndex;

            await _db.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> DeleteRubricAsync(int id)
        {
            var rubric = await _db.Rubrics.FindAsync(id);

            if (rubric == null)
                return Result.Failure(Error.NotFound("RUB404", "Rubric not found."));

            _db.Rubrics.Remove(rubric);
            await _db.SaveChangesAsync();

            return Result.Success();
        }
        public async Task<Result<RubricResponse>> GetRubricByIdAsync(int id)
        {
            var rubric = await _db.Rubrics.FindAsync(id);

            if (rubric is null)
                return Result.Failure<RubricResponse>(
                    Error.NotFound("RUB404", "Rubric not found.")
                );

            return new RubricResponse(
                rubric.Id,
                rubric.Criteria,
                rubric.MaxScore,
                rubric.OrderIndex,
                rubric.ExamId
            );
        }

        public async Task<Result<List<RubricResponse>>> GetRubricsByExamIdAsync(int examId)
        {
            bool examExists = await _db.Exams.AnyAsync(x => x.Id == examId);
            if (!examExists)
            {
                return Result.Failure<List<RubricResponse>>(
                    Error.NotFound("EXAM40401", "Exam not found.")
                );
            }

            var list = await _db.Rubrics
                .Where(x => x.ExamId == examId)
                .OrderBy(x => x.OrderIndex)
                .Select(x => new RubricResponse(
                    x.Id,
                    x.Criteria,
                    x.MaxScore,
                    x.OrderIndex,
                    x.ExamId
                ))
                .ToListAsync();

            return list;
        }

        public async Task<Result<List<RubricResponse>>> GetRubricsAsync(int? examId)
        {
            var query = _db.Rubrics.AsQueryable();

            if (examId.HasValue)
                query = query.Where(r => r.ExamId == examId.Value);

            var rubrics = await query
                .OrderBy(r => r.OrderIndex)
                .ToListAsync();

            var result = rubrics.Select(r => new RubricResponse(
                r.Id,
                r.Criteria,
                r.MaxScore,
                r.OrderIndex,
                r.ExamId
            )).ToList();

            return result;
        }
    }
}
