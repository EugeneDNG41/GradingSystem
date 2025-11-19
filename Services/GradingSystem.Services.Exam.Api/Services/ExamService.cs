using GradingSystem.Services.Exams.Api.Data;
using GradingSystem.Services.Exams.Api.Models;
using GradingSystem.Shared;
using Microsoft.EntityFrameworkCore;
using Wolverine;

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
        public async Task<Result<ExamDetailResponse>> GetExamByIdAsync(int id)
        {
            var exam = await _db.Exams
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null)
                return Result.Failure<ExamDetailResponse>(
                    Error.NotFound("EXAM404", "Exam not found"));

            var semester = await _db.Semesters
                .FirstOrDefaultAsync(s => s.Id == exam.SemesterId);

            if (semester == null)
                return Result.Failure<ExamDetailResponse>(
                    Error.NotFound("SEM404", "Semester not found"));

            var semesterInfo = new SemesterInfo(
                semester.Id,
                semester.Name,
                semester.StartDate,
                semester.EndDate
            );

            var rubrics = await _db.Rubrics
                .Where(r => r.ExamId == exam.Id)
                .Select(r => new RubricInfo(
                    r.Id,
                    r.Criteria,
                    r.MaxScore,
                    r.OrderIndex
                ))
                .ToListAsync();

            var examinerIds = await _db.ExamExaminers
                .Where(x => x.ExamId == exam.Id)
                .Select(x => x.UserId)
                .ToListAsync();

            var examiners = await _db.Examiners
                .Where(ex => examinerIds.Contains(ex.Id))
                .Select(ex => new ExaminerInfo(
                    ex.Id,
                    ex.Name    
                ))
                .ToListAsync();

            var response = new ExamDetailResponse(
                exam.Id,
                exam.Title,
                exam.DueDate,
                semesterInfo,
                rubrics,
                examiners
            );

            return response;
        }
        public async Task<Result<List<ExamListItem>>> GetExamListAsync(int? semesterId)
        {
            var query = _db.Exams.AsQueryable();

            if (semesterId.HasValue)
                query = query.Where(x => x.SemesterId == semesterId.Value);

            var exams = await query
                .OrderBy(e => e.DueDate)
                .ToListAsync();

            if (exams.Count == 0)
                return new List<ExamListItem>();

            var semesterIds = exams.Select(e => e.SemesterId).Distinct().ToList();

            var semesters = await _db.Semesters
                .Where(s => semesterIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id);

            var examinerCounts = await _db.ExamExaminers
                .GroupBy(x => x.ExamId)
                .Select(g => new { ExamId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ExamId, x => x.Count);

            var rubricCounts = await _db.Rubrics
                .GroupBy(r => r.ExamId)
                .Select(g => new { ExamId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ExamId, x => x.Count);

            var result = exams.Select(e => new ExamListItem(
                e.Id,
                e.Title,
                e.DueDate,
                e.SemesterId,
                semesters[e.SemesterId].Name,
                examinerCounts.ContainsKey(e.Id) ? examinerCounts[e.Id] : 0,
                rubricCounts.ContainsKey(e.Id) ? rubricCounts[e.Id] : 0
            )).ToList();

            return result;
        }


    }
}
