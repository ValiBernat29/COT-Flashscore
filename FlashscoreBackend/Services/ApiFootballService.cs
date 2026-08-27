using System.Text.Json;
using FlashscoreBackend.Data;
using FlashscoreBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FlashscoreBackend.Services;

public class ApiFootballService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<ApiFootballService> _logger;

    public ApiFootballService(HttpClient http, IConfiguration config, ILogger<ApiFootballService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    // Sync Liga 1 teams + full squads into the DB
    public async Task SyncTeamsAndPlayersAsync(AppDbContext db)
    {
        var leagueId = _config.GetValue<int>("ApiFootball:LeagueId");
        var season   = _config.GetValue<int>("ApiFootball:Season");

        _logger.LogInformation("Syncing teams for league {LeagueId} season {Season}...", leagueId, season);

        var teamsResponse = await GetAsync($"teams?league={leagueId}&season={season}");
        if (teamsResponse is null) return;

        var teamItems = teamsResponse.Value
            .GetProperty("response")
            .EnumerateArray()
            .ToList();

        _logger.LogInformation("Found {Count} teams from API-Football.", teamItems.Count);

        foreach (var item in teamItems)
        {
            var teamData = item.GetProperty("team");
            var apiId    = teamData.GetProperty("id").GetInt32();
            var name     = teamData.GetProperty("name").GetString() ?? "Unknown";
            var logo     = teamData.GetProperty("logo").GetString() ?? "";

            var team = await db.Teams.FirstOrDefaultAsync(t => t.ApiFootballId == apiId);
            if (team is null)
            {
                team = new Team { ApiFootballId = apiId, Name = name, LogoUrl = logo };
                db.Teams.Add(team);
                await db.SaveChangesAsync();
                _logger.LogInformation("  Created team: {Name}", name);
            }
            else
            {
                team.Name    = name;
                team.LogoUrl = logo;
                await db.SaveChangesAsync();
                _logger.LogInformation("  Updated team: {Name}", name);
            }

            await Task.Delay(200);
            var squadResponse = await GetAsync($"players/squads?team={apiId}");
            if (squadResponse is null) continue;

            var responseArray = squadResponse.Value.GetProperty("response").EnumerateArray().ToList();
            if (responseArray.Count == 0) continue;

            var squadItems = responseArray[0]
                .GetProperty("players")
                .EnumerateArray()
                .ToList();

            _logger.LogInformation("  -> {Count} players for {Name}", squadItems.Count, name);

            foreach (var pData in squadItems)
            {
                var playerId   = pData.GetProperty("id").GetInt32();
                var playerName = pData.GetProperty("name").GetString() ?? "Unknown";
                int number     = GetIntOrDefault(pData, "number", 0);
                var position   = MapPosition(pData.GetProperty("position").GetString());
                var photo      = pData.GetProperty("photo").GetString() ?? "";
                int? age       = GetNullableInt(pData, "age");

                var player = await db.Players.FirstOrDefaultAsync(p => p.ApiFootballId == playerId);
                if (player is null)
                {
                    player = new Player
                    {
                        ApiFootballId = playerId,
                        Name     = playerName,
                        Number   = number,
                        Position = position,
                        PhotoUrl = photo,
                        Age      = age,
                        TeamId   = team.Id,
                    };
                    db.Players.Add(player);
                }
                else
                {
                    player.Name     = playerName;
                    player.Number   = number;
                    player.Position = position;
                    player.PhotoUrl = photo;
                    player.Age      = age;
                    player.TeamId   = team.Id;
                }
            }

            await db.SaveChangesAsync();
        }

        _logger.LogInformation("Sync complete.");
    }

    private async Task<JsonElement?> GetAsync(string endpoint)
    {
        var baseUrl = _config["ApiFootball:BaseUrl"];
        var apiKey  = _config["ApiFootball:ApiKey"];

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/{endpoint}");
            request.Headers.Add("x-apisports-key", apiKey);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API-Football returned {Status} for /{Endpoint}", response.StatusCode, endpoint);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonElement>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling API-Football endpoint: {Endpoint}", endpoint);
            return null;
        }
    }

    private static string MapPosition(string? apiPosition) => apiPosition switch
    {
        "Goalkeeper"  => "GK",
        "Defender"    => "DEF",
        "Midfielder"  => "MID",
        "Attacker"    => "FWD",
        _             => apiPosition ?? "---",
    };

    private static int GetIntOrDefault(JsonElement el, string prop, int fallback)
    {
        if (el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null)
            return v.GetInt32();
        return fallback;
    }

    private static int? GetNullableInt(JsonElement el, string prop)
    {
        if (el.TryGetProperty(prop, out var v) && v.ValueKind != JsonValueKind.Null)
            return v.GetInt32();
        return null;
    }
}
