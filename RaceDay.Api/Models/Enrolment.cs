namespace RaceDay.Api.Models;

public class Enrolment
{
    public int Id { get; set; }
    public int ParticipantId { get; set; }
    public User Participant { get; set; } = null!;
    public int EventCategoryId { get; set; }
    public EventCategory EventCategory { get; set; } = null!;
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Confirmed";

    public Result? Result { get; set; }
}
