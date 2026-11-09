namespace RaceDay.Api.DTOs;

public record CreateEventRequest(string Title, string? Description, DateTime EventDate, string Location, double Distance, string EventType);
public record UpdateEventRequest(string? Title, string? Description, DateTime? EventDate, string? Location, double? Distance, string? EventType, string? Status);
public record EventResponse(int Id, string Title, string? Description, DateTime EventDate, string Location, double Distance, string EventType, string Status, string Organiser);
