using GradingSystem.Services.Submissions.Api.Data;
using GradingSystem.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace GradingSystem.Services.Submissions.Api.Services
{
    public class ExaminerAssignedHandler
    {
        private readonly SubmissionsDbContext _db;

        public ExaminerAssignedHandler(SubmissionsDbContext db)
        {
            _db = db;
        }

        public async Task Handle(ExaminerAssignedToExam message)
        {
            bool exists = await _db.AssignedExaminers
                .AnyAsync(x => x.ExamId == message.ExamId
                            && x.ExaminerId == message.ExaminerId);

            if (exists)
                return;

            _db.AssignedExaminers.Add(new AssignedExaminer
            {
                ExamId = message.ExamId,
                ExaminerId = message.ExaminerId,
                AssignedAt = message.AssignedAt
            });

            await _db.SaveChangesAsync();
        }
    }

}
