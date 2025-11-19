using GradingSystem.Services.Submissions.Api.Data;
using GradingSystem.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Submissions.Api.Services
{
    public class CachedExaminerCreatedHandler(SubmissionsDbContext dbContext)
    {

        public async Task Handle(UserCreated message)
        {
            if (message.Role != "Examiner")
            {
                return;
            }
            var examiner = new CachedExaminer
            {
                Id = message.Id,
                Name = message.Name
            };
            dbContext.CachedExaminers.Add(examiner);
            await dbContext.SaveChangesAsync();
        }
    }
}
