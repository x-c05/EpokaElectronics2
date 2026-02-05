using EpokaElectronics.Api.Data;
using EpokaElectronics.Api.Dtos;
using EpokaElectronics.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EpokaElectronics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CategoriesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var items = await _db.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CategoryDto(x.Id, x.Name))
            .ToListAsync();

        return Ok(items);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryDto request)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest();

        var exists = await _db.Categories.AnyAsync(x => x.Name.ToLower() == name.ToLower());
        if (exists)
            return Conflict(new { message = "Category already exists." });

        var cat = new Category { Name = name };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { }, new CategoryDto(cat.Id, cat.Name));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cat = await _db.Categories.FindAsync(id);
        if (cat is null)
            return NotFound();

        var inUse = await _db.Products.AnyAsync(p => p.CategoryId == id);
        if (inUse)
            return Conflict(new { message = "Category has products." });

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
