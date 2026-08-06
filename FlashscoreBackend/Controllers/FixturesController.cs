using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashscoreBackend.Data;
using FlashscoreBackend.Models;

namespace FlashscoreBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FixturesController : ControllerBase
{
    private readonly AppDbContext _context;

    public FixturesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Fixture>>> GetFixtures()
    {
        return await _context.Fixtures
            .Include(f => f.HomeTeam)
            .Include(f => f.AwayTeam)
            .AsNoTracking()
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Fixture>> GetFixture(int id)
    {
        var fixture = await _context.Fixtures
            .Include(f => f.HomeTeam)
            .Include(f => f.AwayTeam)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id);

        if (fixture == null) return NotFound();
        return fixture;
    }

    [HttpPost]
    public async Task<ActionResult<Fixture>> CreateFixture(Fixture fixture)
    {
        if (fixture.HomeTeamId == fixture.AwayTeamId)
        {
            return BadRequest("A team cannot play against itself.");
        }

        fixture.HomeTeam = null;
        fixture.AwayTeam = null;

        _context.Fixtures.Add(fixture);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFixture), new { id = fixture.Id }, fixture);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFixture(int id, Fixture updatedFixture)
    {
        if (id != updatedFixture.Id) return BadRequest();

        _context.Entry(updatedFixture).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!FixtureExists(id)) return NotFound();
            else throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFixture(int id)
    {
        var fixture = await _context.Fixtures.FindAsync(id);
        if (fixture == null) return NotFound();

        _context.Fixtures.Remove(fixture);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private bool FixtureExists(int id)
    {
        return _context.Fixtures.Any(e => e.Id == id);
    }
}