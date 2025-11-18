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
    }
}
