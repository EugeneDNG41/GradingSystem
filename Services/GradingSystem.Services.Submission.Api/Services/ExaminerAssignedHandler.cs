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
            // Do not create exam-level assignment (SubmissionBatchId = null) in Submissions service
            // Exam assignment is managed in Exams service only
            // Batch-level assignments should be created explicitly via /submissions/batches/assign-examiner endpoint
            // This prevents creating duplicate exam-level assignments when assigning to specific batches
            await Task.CompletedTask;
        }
    }

}
