using System.ComponentModel.DataAnnotations;

namespace LigaVoleibol.API.DTOs;

public record PlayerResponse(int Id, string FirstName, string LastName, string? Position, int JerseyNumber, int TeamId, string? PhotoUrl);

public record PlayerRequest(
    [Required, MaxLength(80)] string FirstName,
    [Required, MaxLength(80)] string LastName,
    [MaxLength(30)] string? Position,
    [Range(1, 99)] int JerseyNumber,
    int TeamId,
    [MaxLength(500)] string? PhotoUrl
);
