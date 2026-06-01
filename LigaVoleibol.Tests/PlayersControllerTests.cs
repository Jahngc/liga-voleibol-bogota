using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using LigaVoleibol.API.DTOs;

namespace LigaVoleibol.Tests;

public class PlayersControllerTests
{
    private static HttpClient CreateClient() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.UseEnvironment("Testing"))
            .CreateClient();

    [Fact]
    public async Task GetPlayers_WithTeamId_ReturnsFilteredList()
    {
        var client = CreateClient();
        var team1 = await CreateTeam(client, "Equipo A");
        var team2 = await CreateTeam(client, "Equipo B");
        await CreatePlayer(client, "Juan", "García", team1.Id, 5);
        await CreatePlayer(client, "Pedro", "López", team2.Id, 8);

        var response = await client.GetAsync($"/api/players?teamId={team1.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var players = await response.Content.ReadFromJsonAsync<List<PlayerResponse>>();
        Assert.Single(players!);
        Assert.Equal(team1.Id, players![0].TeamId);
    }

    [Fact]
    public async Task GetPlayers_WithoutFilter_ReturnsAll()
    {
        var client = CreateClient();
        var team = await CreateTeam(client, "Equipo C");
        await CreatePlayer(client, "Ana", "Ruiz", team.Id, 3);
        await CreatePlayer(client, "María", "Castro", team.Id, 7);

        var response = await client.GetAsync("/api/players");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var players = await response.Content.ReadFromJsonAsync<List<PlayerResponse>>();
        Assert.NotNull(players);
        Assert.True(players!.Count >= 2);
    }

    [Fact]
    public async Task PostPlayer_ReturnsCreated()
    {
        var client = CreateClient();
        var team = await CreateTeam(client, "Equipo D");
        var request = new PlayerRequest("Carlos", "Pérez", "Libero", 10, team.Id, null);
        var response = await client.PostAsJsonAsync("/api/players", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<PlayerResponse>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal(team.Id, created.TeamId);
    }

    [Fact]
    public async Task PostPlayer_ReturnsBadRequest_WhenTeamNotFound()
    {
        var client = CreateClient();
        var request = new PlayerRequest("X", "Y", null, 1, 99999, null);
        var response = await client.PostAsJsonAsync("/api/players", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutPlayer_ReturnsOk_WhenExists()
    {
        var client = CreateClient();
        var team = await CreateTeam(client, "Equipo E");
        var player = await CreatePlayer(client, "Nombre", "Apellido", team.Id, 4);
        var update = new PlayerRequest("NuevoNombre", "NuevoApellido", "Central", 11, team.Id, null);
        var response = await client.PutAsJsonAsync($"/api/players/{player.Id}", update);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<PlayerResponse>();
        Assert.Equal("NuevoNombre", updated!.FirstName);
    }

    [Fact]
    public async Task PutPlayer_ReturnsNotFound_WhenMissing()
    {
        var client = CreateClient();
        var team = await CreateTeam(client, "Equipo F");
        var update = new PlayerRequest("X", "Y", null, 1, team.Id, null);
        var response = await client.PutAsJsonAsync("/api/players/99999", update);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeletePlayer_ReturnsNoContent_WhenExists()
    {
        var client = CreateClient();
        var team = await CreateTeam(client, "Equipo G");
        var player = await CreatePlayer(client, "Para", "Eliminar", team.Id, 6);
        var response = await client.DeleteAsync($"/api/players/{player.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeletePlayer_ReturnsNotFound_WhenMissing()
    {
        var client = CreateClient();
        var response = await client.DeleteAsync("/api/players/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<TeamResponse> CreateTeam(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/teams",
            new TeamRequest(name, null, null, null));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TeamResponse>())!;
    }

    private static async Task<PlayerResponse> CreatePlayer(HttpClient client, string first, string last, int teamId, int jersey)
    {
        var response = await client.PostAsJsonAsync("/api/players",
            new PlayerRequest(first, last, null, jersey, teamId, null));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PlayerResponse>())!;
    }
}
