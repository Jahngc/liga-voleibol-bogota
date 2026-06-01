using System.ComponentModel.DataAnnotations;

namespace LigaVoleibol.API.DTOs;

public record TeamResponse(int Id, string Name, string? Category, string? Venue, string? LogoUrl, DateTime CreatedAt);

public record TeamRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(50)] string? Category,
    [MaxLength(150)] string? Venue,
    [MaxLength(500)] string? LogoUrl
);
