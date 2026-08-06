using System.Text.Json.Serialization;

namespace FlashscoreBackend.Models;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Position { get; set; } = string.Empty;

    // Foreign Key linking to the Team
    public int TeamId { get; set; }

    // Navigation property
    [JsonIgnore]
    public Team? Team { get; set; }
}