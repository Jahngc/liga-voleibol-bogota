using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaVoleibol.API.Data;
using LigaVoleibol.API.DTOs;
using LigaVoleibol.API.Models;

namespace LigaVoleibol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly AppDbContext _db;

    public MatchesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<MatchResponse>>> GetAll(
        [FromQuery] int? teamId,
        [FromQuery] string? status)
    {
        var query = _db.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .AsQueryable();

        if (teamId.HasValue)
            query = query.Where(m => m.HomeTeamId == teamId.Value || m.AwayTeamId == teamId.Value);

        if (!string.IsNullOrEmpty(status))
        {
            if (!Enum.TryParse<MatchStatus>(status, true, out var parsedStatus))
                return BadRequest(new { error = $"Invalid status '{status}'. Valid values: Scheduled, Completed." });
            query = query.Where(m => m.Status == parsedStatus);
        }

        var matches = await query.Select(m => ToResponse(m)).ToListAsync();
        return Ok(matches);
    }

    [HttpPost]
    public async Task<ActionResult<MatchResponse>> Create(MatchRequest request)
    {
        if (request.HomeTeamId == request.AwayTeamId)
            return BadRequest(new { error = "Home and away teams must be different." });

        var homeExists = await _db.Teams.AnyAsync(t => t.Id == request.HomeTeamId);
        var awayExists = await _db.Teams.AnyAsync(t => t.Id == request.AwayTeamId);

        if (!homeExists || !awayExists)
            return BadRequest(new { error = "One or both teams not found." });

        var match = new Match
        {
            HomeTeamId = request.HomeTeamId,
            AwayTeamId = request.AwayTeamId,
            ScheduledAt = request.ScheduledAt,
            Venue = request.Venue,
            Status = MatchStatus.Scheduled
        };
        _db.Matches.Add(match);
        await _db.SaveChangesAsync();

        await _db.Entry(match).Reference(m => m.HomeTeam).LoadAsync();
        await _db.Entry(match).Reference(m => m.AwayTeam).LoadAsync();

        return CreatedAtAction(nameof(GetAll), new { }, ToResponse(match));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MatchResponse>> Update(int id, MatchRequest request)
    {
        var match = await _db.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (match is null) return NotFound();

        if (match.Status == MatchStatus.Completed)
            return Conflict(new { error = "Cannot update a completed match." });

        if (request.HomeTeamId == request.AwayTeamId)
            return BadRequest(new { error = "Home and away teams must be different." });

        match.HomeTeamId = request.HomeTeamId;
        match.AwayTeamId = request.AwayTeamId;
        match.ScheduledAt = request.ScheduledAt;
        match.Venue = request.Venue;
        await _db.SaveChangesAsync();

        await _db.Entry(match).Reference(m => m.HomeTeam).LoadAsync();
        await _db.Entry(match).Reference(m => m.AwayTeam).LoadAsync();

        return Ok(ToResponse(match));
    }

    [HttpPatch("{id}/result")]
    public async Task<ActionResult<MatchResponse>> RegisterResult(int id, MatchResultRequest request)
    {
        var match = await _db.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (match is null) return NotFound();

        if (match.Status == MatchStatus.Completed)
            return Conflict(new { error = "Match result has already been registered." });

        match.HomeScore = request.HomeScore;
        match.AwayScore = request.AwayScore;
        match.Status = MatchStatus.Completed;
        await _db.SaveChangesAsync();

        return Ok(ToResponse(match));
    }

    private static MatchResponse ToResponse(Match m) =>
        new(m.Id,
            m.HomeTeamId, m.HomeTeam?.Name ?? string.Empty,
            m.AwayTeamId, m.AwayTeam?.Name ?? string.Empty,
            m.ScheduledAt, m.Venue,
            m.HomeScore, m.AwayScore,
            m.Status.ToString());
}
