using GradingSystem.Services.Exams.Api.Data;
using GradingSystem.Services.Exams.Api.Models;
using GradingSystem.Shared;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Exams.Api.Services
{
    public class ExamService : IExamService
    {
        private readonly ExamsDbContext _db;

        public ExamService(ExamsDbContext db)
        {
            _db = db;
        }

        public async Task<Result<int>> CreateExamAsync(CreateExamRequest request)
        {
            bool exists = await _db.Semesters.AnyAsync(x => x.Id == request.SemesterId);
            if (!exists)
            {
                return Result.Failure<int>(
                    Error.NotFound("SEM40401", "Semester not found.")
                );
            }

            var exam = new Exam
            {
                Title = request.Title,
                DueDate = request.DueDate,
                SemesterId = request.SemesterId
            };

            _db.Exams.Add(exam);
            await _db.SaveChangesAsync();

            return exam.Id;
        }
    }
}
