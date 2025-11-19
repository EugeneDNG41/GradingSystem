namespace GradingSystem.Services.Submissions.Api.Data
{
    public class AssignedExaminer
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public int ExaminerId { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
