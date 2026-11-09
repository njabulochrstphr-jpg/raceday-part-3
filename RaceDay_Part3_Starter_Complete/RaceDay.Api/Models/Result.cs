namespace RaceDay.Api.Models;

public class Result
{
    public int Id { get; set; }
    public int EnrolmentId { get; set; }
    public Enrolment Enrolment { get; set; } = null!;
    public string FinishTime { get; set; } = string.Empty;
    public int Position { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
