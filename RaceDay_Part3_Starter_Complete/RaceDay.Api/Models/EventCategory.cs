namespace RaceDay.Api.Models;

public class EventCategory
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
