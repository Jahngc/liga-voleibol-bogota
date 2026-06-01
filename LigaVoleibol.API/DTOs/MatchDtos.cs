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

// HomeScore/AwayScore represent sets won (0-3 typical, max 5 in best-of-5 format)
public record MatchResultRequest(
    [Range(0, 5)] int HomeScore,
    [Range(0, 5)] int AwayScore
);
