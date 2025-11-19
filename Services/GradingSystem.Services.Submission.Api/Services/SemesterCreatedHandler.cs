using GradingSystem.Services.Submissions.Api.Data;
using GradingSystem.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

public class SemesterCreatedHandler
{
    private readonly SubmissionsDbContext _db;

    public SemesterCreatedHandler(SubmissionsDbContext db)
    {
        _db = db;
    }

    public async Task Handle(SemesterCreated message)
    {
        bool exists = await _db.CachedSemesters.AnyAsync(x => x.Id == message.Id);
        if (exists)
            return;

        var cachedsemester = new CachedSemester
        {
            Id = message.Id,
            Name = message.Name,
            StartDate = message.StartDate,
            EndDate = message.EndDate
        };

        _db.CachedSemesters.Add(cachedsemester);
        await _db.SaveChangesAsync();
    }
}
