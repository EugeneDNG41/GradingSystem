using GradingSystem.Services.Exams.Api.Data;
using GradingSystem.Shared;

namespace GradingSystem.Services.Exams.Api.Services
{
    public interface IExamExaminerService
    {
        public Task<IReadOnlyList<ExamExaminer>>GetAllExamsByExaminerIdAsync(int examinerId);
        public Task<Result> AsignExaminer(ExamExaminer examExaminer);
    }
}