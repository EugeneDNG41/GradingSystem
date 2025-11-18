using GradingSystem.Services.Exams.Api.Data;
using GradingSystem.Shared.Contracts;

namespace GradingSystem.Services.Exams.Api.Services;

public class ExaminerCreatedHandler(ExamsDbContext dbContext)

{
    public async Task Handle(UserCreated message)
    {
        if (message.Role != "Examiner")
        {
            return;
        }
        var examiner = new Examiner
        {
            Id = message.Id,
            Name = message.Name
        };
        dbContext.Examiners.Add(examiner);
        await dbContext.SaveChangesAsync();
    }
}
