using System.ComponentModel.DataAnnotations;

namespace LigaVoleibol.API.Models;

public enum MatchStatus { Scheduled, Completed }

public class Match
{
    public int Id { get; set; }

    public int HomeTeamId { get; set; }
    public Team HomeTeam { get; set; } = null!;

    public int AwayTeamId { get; set; }
    public Team AwayTeam { get; set; } = null!;

    public DateTime ScheduledAt { get; set; }

    [MaxLength(150)]
    public string? Venue { get; set; }

    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }

    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
}
