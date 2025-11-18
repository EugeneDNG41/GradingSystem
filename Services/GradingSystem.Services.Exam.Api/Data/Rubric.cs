namespace GradingSystem.Services.Exams.Api.Data
{
    public class Rubric
    {
        public int Id { get; set; }
        public string Criteria { get; set; } = null!;
        public decimal MaxScore { get; set; }
        public int OrderIndex { get; set; }
    }
}
