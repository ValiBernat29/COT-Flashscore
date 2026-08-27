using System.Text.Json.Serialization;

namespace FlashscoreBackend.Models;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Position { get; set; } = string.Empty;
    public int? ApiFootballId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public int? Age { get; set; }

    public int TeamId { get; set; }

    [JsonIgnore]
    public Team? Team { get; set; }
}