using GradingSystem.Services.Exams.Api.Data;
using GradingSystem.Services.Exams.Api.Models;
using GradingSystem.Shared;
using GradingSystem.Shared.Contracts;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.Runtime;

namespace GradingSystem.Services.Exams.Api.Services
{
    public class SemesterService : ISemesterService
    {
        private readonly ExamsDbContext _db;
        private readonly IMessageBus _messageBus;

        public SemesterService(ExamsDbContext db, IMessageBus messageBus)
        {
            _db = db;
            _messageBus = messageBus;

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
            await _messageBus.PublishAsync(
                new SemesterCreated(
                    semester.Id,
                    semester.Name,
                    semester.StartDate,
                    semester.EndDate
                )
            );
            return semester.Id;
        }
        public async Task<Result<SemesterResponse>> GetSemesterByIdAsync(int id)
        {
            var semester = await _db.Semesters.FindAsync(id);

            if (semester is null)
                return Result.Failure<SemesterResponse>(Error.NotFound("S40401", "Semester not found"));

            return Result.Success(new SemesterResponse(id,
                semester.Name,
                semester.StartDate,
                semester.EndDate));
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

 

        public async Task<Result<List<SemesterResponse>>> GetAllSemestersAsync()
        {
            var list = await _db.Semesters
                .Select(x => new SemesterResponse(
                    x.Id,
                    x.Name,
                    x.StartDate,
                    x.EndDate))
                .ToListAsync();

            return Result.Success(list);
        }

        public async Task<Result<SemesterResponse>> UpdateSemesterAsync(int id, UpdateSemesterRequest request)
        {
            var semester = await _db.Semesters.FindAsync(id);

            if (semester is null)
                return Result.Failure<SemesterResponse>(Error.NotFound("S40401", "Semester not found"));

            semester.Name = request.Name;
            semester.StartDate = request.StartDate;
            semester.EndDate = request.EndDate;

            await _db.SaveChangesAsync();

            return Result.Success(new SemesterResponse(
                semester.Id,
                semester.Name,
                semester.StartDate,
                semester.EndDate
            ));
        }

        public async Task<Result<bool>> DeleteSemesterAsync(int id)
        {
            var semester = await _db.Semesters.FindAsync(id);

            if (semester is null)
                return Result.Failure<bool>(Error.NotFound("S40401", "Semester not found"));

            _db.Semesters.Remove(semester);
            await _db.SaveChangesAsync();

            return Result.Success(true);
        }
        }
        
    }
