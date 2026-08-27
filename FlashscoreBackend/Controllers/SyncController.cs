using Microsoft.AspNetCore.Mvc;
using FlashscoreBackend.Data;
using FlashscoreBackend.Services;

namespace FlashscoreBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly ApiFootballService _apiFootball;
    private readonly AppDbContext _db;

    public SyncController(ApiFootballService apiFootball, AppDbContext db)
    {
        _apiFootball = apiFootball;
        _db = db;
    }

    /// <summary>
    /// Syncs all Liga 1 teams and their squads from API-Football into the local DB.
    /// Safe to call multiple times — uses upsert logic.
    /// </summary>
    [HttpPost("teams")]
    public async Task<IActionResult> SyncTeams()
    {
        try
        {
            await _apiFootball.SyncTeamsAndPlayersAsync(_db);
            return Ok(new { message = "Teams and players synced successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Sync failed.", error = ex.Message });
        }
    }
}
