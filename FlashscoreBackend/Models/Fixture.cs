using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashscoreBackend.Models;

public class Fixture
{
    public int Id { get; set; }
    public int Matchday { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public List<int> HomeLineup { get; set; } = new();
    public List<int> AwayLineup { get; set; } = new();
    public Team? HomeTeam { get; set; }
    public Team? AwayTeam { get; set; }
    public string Status { get; set; } = MatchStatus.Scheduled;
    public string EventsJson { get; set; } = "[]";

    [NotMapped]
    public List<MatchEvent> Events
    {
        get => string.IsNullOrWhiteSpace(EventsJson)
            ? new List<MatchEvent>()
            : JsonSerializer.Deserialize<List<MatchEvent>>(EventsJson)!;

        set => EventsJson = JsonSerializer.Serialize(value);

    }
}