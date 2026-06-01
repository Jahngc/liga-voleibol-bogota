using System.ComponentModel.DataAnnotations;
using LigaVoleibol.API.Models;

namespace LigaVoleibol.API.DTOs;

public record MatchResponse(
    int Id,
    int HomeTeamId, string HomeTeamName,
    int AwayTeamId, string AwayTeamName,
    DateTime ScheduledAt,
    string? Venue,
    int? HomeScore, int? AwayScore,
    string Status
);

public record MatchRequest(
    int HomeTeamId,
    int AwayTeamId,
    DateTime ScheduledAt,
    [MaxLength(150)] string? Venue
);

public record MatchResultRequest(
    [Range(0, 5)] int HomeScore,
    [Range(0, 5)] int AwayScore
);
