namespace RaceDay.Api.Models;

public class Event
{
    public int Id { get; set; }
    public int OrganiserId { get; set; }
    public User Organiser { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public double Distance { get; set; }
    public string EventType { get; set; } = string.Empty; // run, walk, cycle
    public string Status { get; set; } = "Scheduled";

    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
}
