namespace RaceDay.Api.DTOs;

public record UpdateProfileRequest(string? Name, string? Email);
public record UserProfileResponse(int Id, string Name, string Email, string Role);
