using GradingSystem.Services.Exams.Api.Data;
using Wolverine;
using Microsoft.EntityFrameworkCore;
using GradingSystem.Shared;

namespace GradingSystem.Services.Exams.Api.Services
{
    public class ExamExaminerService
        (
        ExamsDbContext dbContext,
        IMessageBus messageBus

        )
        
        : IExamExaminerService
    {
        public Task<IReadOnlyList<ExamExaminer>> GetAllExamsByExaminerIdAsync(int examinerId)
        {
            throw new NotImplementedException();
        }
        public async Task<Result> AsignExaminer(ExamExaminer examExaminer)
        {
            var exists = await dbContext.Set<ExamExaminer>()
                .AnyAsync(ee => ee.ExamId == examExaminer.ExamId && ee.UserId == examExaminer.UserId);
            if (!exists)
            {
                return Result.Failure(Error.NotFound("404","Not Found"));
            }
            await messageBus.InvokeAsync(new ExaminerExists(examExaminer.UserId));
            throw new NotImplementedException();
        }

    }
}
