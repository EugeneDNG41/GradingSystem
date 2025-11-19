using GradingSystem.Services.Submissions.Api.Data;
using GradingSystem.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

public class ExamCreatedHandler
{
    private readonly SubmissionsDbContext _db;

    public ExamCreatedHandler(SubmissionsDbContext db)
    {
        _db = db;
    }

    public async Task Handle(ExamCreated message)
    {
        bool exists = await _db.CachedExams.AnyAsync(x => x.Id == message.Id);
        if (exists)
            return;

        var CachedExams = new CachedExam
        {
            Id = message.Id,
            Title = message.Title,
            SemesterId = message.SemesterId,
            DueDate = message.DueDate
        };

        _db.CachedExams.Add(CachedExams);
        await _db.SaveChangesAsync();
    }
}
