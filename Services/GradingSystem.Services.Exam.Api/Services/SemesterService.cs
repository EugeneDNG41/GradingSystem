using GradingSystem.Services.Exams.Api.Data;
using GradingSystem.Services.Exams.Api.Models;
using GradingSystem.Shared;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Exams.Api.Services
{
    public class SemesterService : ISemesterService
    {
        private readonly ExamsDbContext _db;

        public SemesterService(ExamsDbContext db)
        {
            _db = db;
        }

        public async Task<Result<int>> CreateSemesterAsync(CreateSemesterRequest request)
        {
            var semester = new Semester
            {
                Name = request.Name,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            _db.Semesters.Add(semester);
            await _db.SaveChangesAsync();

            return semester.Id;
        }

        public async Task<Result<List<SemesterResponse>>> GetSemestersAsync()
        {
            var semesters = await _db.Semesters
                .OrderBy(s => s.StartDate)
                .ToListAsync();

            var result = semesters.Select(s => new SemesterResponse(
                s.Id,
                s.Name,
                s.StartDate,
                s.EndDate
            )).ToList();

            return result;
        }

        public async Task<Result<SemesterResponse>> GetSemesterByIdAsync(int id)
        {
            var semester = await _db.Semesters.FindAsync(id);
            if (semester is null)
            {
                return Result.Failure<SemesterResponse>(Error.NotFound("SEM40401", "Semester not found."));
            }

            var response = new SemesterResponse(
                semester.Id,
                semester.Name,
                semester.StartDate,
                semester.EndDate
            );
            return response;
        }
    }
}
