using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using LigaVoleibol.API.DTOs;

namespace LigaVoleibol.Tests;

public class MatchesControllerTests
{
    private static HttpClient CreateClient() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.UseEnvironment("Testing"))
            .CreateClient();

    [Fact]
    public async Task GetMatches_ReturnsOkWithList()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/matches");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var matches = await response.Content.ReadFromJsonAsync<List<MatchResponse>>();
        Assert.NotNull(matches);
    }

    [Fact]
    public async Task PostMatch_ReturnsCreated()
    {
        var client = CreateClient();
        var (home, away) = await CreateTwoTeams(client);
        var request = new MatchRequest(home.Id, away.Id, DateTime.UtcNow.AddDays(7), "Coliseo Norte");
        var response = await client.PostAsJsonAsync("/api/matches", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<MatchResponse>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal(home.Id, created.HomeTeamId);
        Assert.Equal("Scheduled", created.Status);
    }

    [Fact]
    public async Task PostMatch_ReturnsBadRequest_WhenSameTeam()
    {
        var client = CreateClient();
        var (home, _) = await CreateTwoTeams(client);
        var request = new MatchRequest(home.Id, home.Id, DateTime.UtcNow.AddDays(1), null);
        var response = await client.PostAsJsonAsync("/api/matches", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostMatch_ReturnsBadRequest_WhenTeamNotFound()
    {
        var client = CreateClient();
        var (home, _) = await CreateTwoTeams(client);
        var request = new MatchRequest(home.Id, 99999, DateTime.UtcNow.AddDays(1), null);
        var response = await client.PostAsJsonAsync("/api/matches", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMatches_FilterByTeamId_ReturnsFiltered()
    {
        var client = CreateClient();
        var (home, away) = await CreateTwoTeams(client);
        var (other1, other2) = await CreateTwoTeams(client, "Equipo X", "Equipo Y");
        await CreateMatch(client, home.Id, away.Id);
        await CreateMatch(client, other1.Id, other2.Id);

        var response = await client.GetAsync($"/api/matches?teamId={home.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var matches = await response.Content.ReadFromJsonAsync<List<MatchResponse>>();
        Assert.Single(matches!);
    }

    [Fact]
    public async Task GetMatches_FilterByStatus_ReturnsFiltered()
    {
        var client = CreateClient();
        var (home, away) = await CreateTwoTeams(client);
        var match = await CreateMatch(client, home.Id, away.Id);
        await client.PatchAsJsonAsync($"/api/matches/{match.Id}/result", new MatchResultRequest(3, 1));

        var scheduledResponse = await client.GetAsync("/api/matches?status=scheduled");
        var scheduledMatches = await scheduledResponse.Content.ReadFromJsonAsync<List<MatchResponse>>();
        Assert.Empty(scheduledMatches!);

        var completedResponse = await client.GetAsync("/api/matches?status=completed");
        var completedMatches = await completedResponse.Content.ReadFromJsonAsync<List<MatchResponse>>();
        Assert.Single(completedMatches!);
    }

    [Fact]
    public async Task PutMatch_ReturnsOk_WhenExists()
    {
        var client = CreateClient();
        var (home, away) = await CreateTwoTeams(client);
        var match = await CreateMatch(client, home.Id, away.Id);
        var newDate = DateTime.UtcNow.AddDays(14);
        var update = new MatchRequest(home.Id, away.Id, newDate, "Nuevo Coliseo");
        var response = await client.PutAsJsonAsync($"/api/matches/{match.Id}", update);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<MatchResponse>();
        Assert.Equal("Nuevo Coliseo", updated!.Venue);
    }

    [Fact]
    public async Task PutMatch_ReturnsNotFound_WhenMissing()
    {
        var client = CreateClient();
        var (home, away) = await CreateTwoTeams(client);
        var update = new MatchRequest(home.Id, away.Id, DateTime.UtcNow, null);
        var response = await client.PutAsJsonAsync("/api/matches/99999", update);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PatchResult_UpdatesScoreAndStatus()
    {
        var client = CreateClient();
        var (home, away) = await CreateTwoTeams(client);
        var match = await CreateMatch(client, home.Id, away.Id);
        var response = await client.PatchAsJsonAsync(
            $"/api/matches/{match.Id}/result",
            new MatchResultRequest(3, 1));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<MatchResponse>();
        Assert.Equal(3, updated!.HomeScore);
        Assert.Equal(1, updated.AwayScore);
        Assert.Equal("Completed", updated.Status);
    }

    [Fact]
    public async Task PatchResult_ReturnsNotFound_WhenMissing()
    {
        var client = CreateClient();
        var response = await client.PatchAsJsonAsync(
            "/api/matches/99999/result",
            new MatchResultRequest(3, 0));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<(TeamResponse home, TeamResponse away)> CreateTwoTeams(
        HttpClient client, string homeName = "Leones", string awayName = "Aguilas")
    {
        var h = await PostJson<TeamResponse>(client, "/api/teams", new TeamRequest(homeName, null, null, null));
        var a = await PostJson<TeamResponse>(client, "/api/teams", new TeamRequest(awayName, null, null, null));
        return (h, a);
    }

    private static async Task<MatchResponse> CreateMatch(HttpClient client, int homeId, int awayId) =>
        await PostJson<MatchResponse>(client, "/api/matches",
            new MatchRequest(homeId, awayId, DateTime.UtcNow.AddDays(7), "Coliseo Central"));

    private static async Task<T> PostJson<T>(HttpClient client, string url, object body)
    {
        var response = await client.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }
}
