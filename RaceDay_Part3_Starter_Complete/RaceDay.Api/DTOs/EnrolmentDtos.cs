namespace RaceDay.Api.DTOs;

public record EnrolmentResponse(int Id, string EventTitle, string Category, DateTime EnrolmentDate, string Status);
