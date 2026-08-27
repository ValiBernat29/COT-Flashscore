namespace FlashscoreBackend.Models;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ApiFootballId { get; set; }
    public string LogoUrl { get; set; } = string.Empty;
    public List<Player> Players { get; set; } = new();
}