using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashscoreBackend.Data;
using FlashscoreBackend.Models;

namespace FlashscoreBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly AppDbContext _context;

    public PlayersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("team/{teamId}")]
    public async Task<ActionResult<IEnumerable<Player>>> GetPlayersByTeam(int teamId)
    {
        return await _context.Players
            .Where(p => p.TeamId == teamId)
            .OrderBy(p => p.Number)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Player>> CreatePlayer(Player player)
    {
        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        return Ok(player);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayer(int id)
    {
        var player = await _context.Players.FindAsync(id);
        if (player == null) return NotFound();

        _context.Players.Remove(player);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}