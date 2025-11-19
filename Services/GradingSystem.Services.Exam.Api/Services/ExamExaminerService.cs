using GradingSystem.Services.Exams.Api.Data;
using GradingSystem.Shared;
using GradingSystem.Shared.Contracts;
using Microsoft.EntityFrameworkCore;
using Wolverine;

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
            if (exists)
            {
                return Result.Failure(Error.Conflict("EE40901","This user already assigned to this exam"));
            }
            var examExists = await dbContext.Set<Exam>()
                .AnyAsync(e => e.Id == examExaminer.ExamId);
            if (!examExists)
            {
                return Result.Failure(Error.NotFound("EX40401", "Exam not found"));
            }
            var userExists = await dbContext.Set<Examiner>()
                .AnyAsync(u => u.Id == examExaminer.UserId);
            if (!userExists)
            {
                return Result.Failure(Error.NotFound("EX40402", "Examiner not found"));
            }
                dbContext.Set<ExamExaminer>().Add(examExaminer);
            dbContext.SaveChanges();
            await messageBus.PublishAsync(new ExaminerAssignedToExam(
                examExaminer.ExamId,
                examExaminer.UserId,
                DateTime.UtcNow
));
            return Result.Success();
        }

    }
}
