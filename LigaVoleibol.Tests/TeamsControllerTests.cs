using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using LigaVoleibol.API.DTOs;

namespace LigaVoleibol.Tests;

public class TeamsControllerTests
{
    private static HttpClient CreateClient() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.UseEnvironment("Testing"))
            .CreateClient();

    [Fact]
    public async Task GetTeams_ReturnsOkWithList()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/teams");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var teams = await response.Content.ReadFromJsonAsync<List<TeamResponse>>();
        Assert.NotNull(teams);
    }

    [Fact]
    public async Task PostTeam_ReturnsCreatedWithId()
    {
        var client = CreateClient();
        var request = new TeamRequest("Panteras FC", "Mixto", "Polideportivo Sur", null);
        var response = await client.PostAsJsonAsync("/api/teams", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TeamResponse>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("Panteras FC", created.Name);
    }

    [Fact]
    public async Task GetTeamById_ReturnsOk_WhenExists()
    {
        var client = CreateClient();
        var created = await CreateTeam(client, "Halcones");
        var response = await client.GetAsync($"/api/teams/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var team = await response.Content.ReadFromJsonAsync<TeamResponse>();
        Assert.Equal("Halcones", team!.Name);
    }

    [Fact]
    public async Task GetTeamById_ReturnsNotFound_WhenMissing()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/teams/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutTeam_ReturnsOk_WhenExists()
    {
        var client = CreateClient();
        var created = await CreateTeam(client, "Original");
        var update = new TeamRequest("Actualizado", "Femenino", "Nuevo Coliseo", null);
        var response = await client.PutAsJsonAsync($"/api/teams/{created.Id}", update);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<TeamResponse>();
        Assert.Equal("Actualizado", updated!.Name);
    }

    [Fact]
    public async Task PutTeam_ReturnsNotFound_WhenMissing()
    {
        var client = CreateClient();
        var update = new TeamRequest("X", null, null, null);
        var response = await client.PutAsJsonAsync("/api/teams/99999", update);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTeam_ReturnsNoContent_WhenExists()
    {
        var client = CreateClient();
        var created = await CreateTeam(client, "ParaEliminar");
        var response = await client.DeleteAsync($"/api/teams/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTeam_ReturnsNotFound_WhenMissing()
    {
        var client = CreateClient();
        var response = await client.DeleteAsync("/api/teams/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTeam_RemovesPlayers_WhenTeamHasPlayers()
    {
        var client = CreateClient();
        var team = await CreateTeam(client, "ConJugadores");
        await client.PostAsJsonAsync("/api/players", new PlayerRequest("A", "B", null, 1, team.Id, null));
        var deleteResponse = await client.DeleteAsync($"/api/teams/{team.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        var playersResponse = await client.GetAsync($"/api/players?teamId={team.Id}");
        var players = await playersResponse.Content.ReadFromJsonAsync<List<PlayerResponse>>();
        Assert.Empty(players!);
    }

    [Fact]
    public async Task PostTeam_ReturnsBadRequest_WhenNameMissing()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/teams", new { Category = "X" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<TeamResponse> CreateTeam(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/teams",
            new TeamRequest(name, null, null, null));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TeamResponse>())!;
    }
}
