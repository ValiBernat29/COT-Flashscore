using System.ComponentModel.DataAnnotations.Schema;

namespace FlashscoreBackend.Models;

[NotMapped]

public class MatchEvent
{
    public int TeamId { get; set; }
    public int Minute { get; set; }
    public int? PlayerId { get; set; }
    public string Type { get; set; } = EventTypes.Goal;

}