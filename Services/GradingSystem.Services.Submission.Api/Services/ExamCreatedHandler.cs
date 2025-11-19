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
        bool exists = await _db.Exams.AnyAsync(x => x.Id == message.Id);
        if (exists)
            return;

        var Exam = new Exam
        {
            Id = message.Id,
            Titile = message.Title,
            SemesterId = message.SemesterId,
            DueDate = message.DueDate
        };

        _db.Exams.Add(Exam);
        await _db.SaveChangesAsync();
    }
}
