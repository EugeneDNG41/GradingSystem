namespace GradingSystem.Services.Exams.Api.Data
{
    public class Semester
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
