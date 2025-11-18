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
    }
}
