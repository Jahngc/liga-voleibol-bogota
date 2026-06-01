using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LigaVoleibol.API.Data;
using LigaVoleibol.API.DTOs;
using LigaVoleibol.API.Models;

namespace LigaVoleibol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TeamsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<TeamResponse>>> GetAll()
    {
        var teams = await _db.Teams
            .Select(t => ToResponse(t))
            .ToListAsync();
        return Ok(teams);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TeamResponse>> GetById(int id)
    {
        var team = await _db.Teams.FindAsync(id);
        return team is null ? NotFound() : Ok(ToResponse(team));
    }

    [HttpPost]
    public async Task<ActionResult<TeamResponse>> Create(TeamRequest request)
    {
        var team = new Team
        {
            Name = request.Name,
            Category = request.Category,
            Venue = request.Venue,
            LogoUrl = request.LogoUrl,
            CreatedAt = DateTime.UtcNow
        };
        _db.Teams.Add(team);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = team.Id }, ToResponse(team));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TeamResponse>> Update(int id, TeamRequest request)
    {
        var team = await _db.Teams.FindAsync(id);
        if (team is null) return NotFound();

        team.Name = request.Name;
        team.Category = request.Category;
        team.Venue = request.Venue;
        team.LogoUrl = request.LogoUrl;
        await _db.SaveChangesAsync();
        return Ok(ToResponse(team));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var team = await _db.Teams.FindAsync(id);
        if (team is null) return NotFound();

        _db.Teams.Remove(team);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static TeamResponse ToResponse(Team t) =>
        new(t.Id, t.Name, t.Category, t.Venue, t.LogoUrl, t.CreatedAt);
}
