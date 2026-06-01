using System.ComponentModel.DataAnnotations;

namespace LigaVoleibol.API.Models;

public class Player
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Position { get; set; }

    [Range(1, 99)]
    public int JerseyNumber { get; set; }

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }
}
