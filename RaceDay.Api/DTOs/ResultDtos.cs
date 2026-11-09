namespace RaceDay.Api.DTOs;

public record CreateResultRequest(string FinishTime, int Position);
public record ResultResponse(int Id, string EventTitle, string Category, string FinishTime, int Position);
