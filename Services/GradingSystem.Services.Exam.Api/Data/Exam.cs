namespace GradingSystem.Services.Exams.Api.Data
{
    public class Exam
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public DateTime DueDate { get; set; }

        public int SemesterId { get; set; }
    }
}
