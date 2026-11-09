namespace RaceDay.Api.DTOs;

public record CreateCategoryRequest(string Name);
public record AttachCategoryRequest(int CategoryId);
public record CategoryResponse(int Id, string Name);
public record EventCategoryResponse(int Id, int CategoryId, string CategoryName);
