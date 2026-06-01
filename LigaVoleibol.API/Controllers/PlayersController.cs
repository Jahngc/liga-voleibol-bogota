using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaVoleibol.API.Data;
using LigaVoleibol.API.DTOs;
using LigaVoleibol.API.Models;

namespace LigaVoleibol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly AppDbContext _db;

    public PlayersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<PlayerResponse>>> GetAll([FromQuery] int? teamId)
    {
        var query = _db.Players.AsQueryable();
        if (teamId.HasValue)
            query = query.Where(p => p.TeamId == teamId.Value);

        var players = await query
            .Select(p => ToResponse(p))
            .ToListAsync();
        return Ok(players);
    }

    [HttpPost]
    public async Task<ActionResult<PlayerResponse>> Create(PlayerRequest request)
    {
        var teamExists = await _db.Teams.AnyAsync(t => t.Id == request.TeamId);
        if (!teamExists)
            return BadRequest(new { error = $"Team with id {request.TeamId} not found." });

        var player = new Player
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Position = request.Position,
            JerseyNumber = request.JerseyNumber,
            TeamId = request.TeamId,
            PhotoUrl = request.PhotoUrl
        };
        _db.Players.Add(player);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { teamId = player.TeamId }, ToResponse(player));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PlayerResponse>> Update(int id, PlayerRequest request)
    {
        var player = await _db.Players.FindAsync(id);
        if (player is null) return NotFound();

        player.FirstName = request.FirstName;
        player.LastName = request.LastName;
        player.Position = request.Position;
        player.JerseyNumber = request.JerseyNumber;
        player.TeamId = request.TeamId;
        player.PhotoUrl = request.PhotoUrl;
        await _db.SaveChangesAsync();
        return Ok(ToResponse(player));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var player = await _db.Players.FindAsync(id);
        if (player is null) return NotFound();

        _db.Players.Remove(player);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static PlayerResponse ToResponse(Player p) =>
        new(p.Id, p.FirstName, p.LastName, p.Position, p.JerseyNumber, p.TeamId, p.PhotoUrl);
}
