namespace GradingSystem.Services.Submissions.Api.Data;

public class CachedExam
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int SemesterId { get; set; }
    public DateTime CachedAt { get; set; }
}


