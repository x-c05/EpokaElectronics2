using EpokaElectronics.Api.Data;
using EpokaElectronics.Api.Dtos;
using EpokaElectronics.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EpokaElectronics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ProductsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductListDto>>> GetAll([FromQuery] int? categoryId, [FromQuery] string? q, [FromQuery] bool? featured)
    {
        var query = _db.Products.AsNoTracking().Include(p => p.Category).AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLowerInvariant();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Brand != null && p.Brand.ToLower().Contains(term)) ||
                p.Sku.ToLower().Contains(term));
        }

        if (featured.HasValue)
            query = query.Where(p => p.IsFeatured == featured.Value);

        var items = await query
            .OrderByDescending(p => p.IsFeatured)
            .ThenBy(p => p.Name)
            .Select(p => new ProductListDto(
                p.Id,
                p.Sku,
                p.Name,
                p.Brand,
                p.Price,
                p.Stock,
                p.ImageUrl,
                p.CategoryId,
                p.Category != null ? p.Category.Name : "",
                p.IsFeatured
            ))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> GetById(int id)
    {
        var p = await _db.Products.AsNoTracking().Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id);
        if (p is null)
            return NotFound();

        return Ok(new ProductDetailDto(
            p.Id,
            p.Sku,
            p.Name,
            p.Brand,
            p.Price,
            p.Stock,
            p.ImageUrl,
            p.Description,
            p.CategoryId,
            p.Category != null ? p.Category.Name : "",
            p.IsFeatured,
            p.CreatedAtUtc
        ));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductDetailDto>> Create([FromBody] ProductUpsertRequest request)
    {
        var sku = request.Sku.Trim();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(name))
            return BadRequest();

        var skuExists = await _db.Products.AnyAsync(x => x.Sku.ToLower() == sku.ToLower());
        if (skuExists)
            return Conflict(new { message = "SKU already exists." });

        var catExists = await _db.Categories.AnyAsync(x => x.Id == request.CategoryId);
        if (!catExists)
            return BadRequest(new { message = "Invalid category." });

        var p = new Product
        {
            Sku = sku,
            Name = name,
            Brand = string.IsNullOrWhiteSpace(request.Brand) ? null : request.Brand.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CategoryId = request.CategoryId,
            IsFeatured = request.IsFeatured,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Products.Add(p);
        await _db.SaveChangesAsync();

        var created = await _db.Products.AsNoTracking().Include(x => x.Category).FirstAsync(x => x.Id == p.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new ProductDetailDto(
            created.Id,
            created.Sku,
            created.Name,
            created.Brand,
            created.Price,
            created.Stock,
            created.ImageUrl,
            created.Description,
            created.CategoryId,
            created.Category != null ? created.Category.Name : "",
            created.IsFeatured,
            created.CreatedAtUtc
        ));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> Update(int id, [FromBody] ProductUpsertRequest request)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p is null)
            return NotFound();

        var sku = request.Sku.Trim();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(name))
            return BadRequest();

        var skuTaken = await _db.Products.AnyAsync(x => x.Id != id && x.Sku.ToLower() == sku.ToLower());
        if (skuTaken)
            return Conflict(new { message = "SKU already exists." });

        var catExists = await _db.Categories.AnyAsync(x => x.Id == request.CategoryId);
        if (!catExists)
            return BadRequest(new { message = "Invalid category." });

        p.Sku = sku;
        p.Name = name;
        p.Brand = string.IsNullOrWhiteSpace(request.Brand) ? null : request.Brand.Trim();
        p.Price = request.Price;
        p.Stock = request.Stock;
        p.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        p.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        p.CategoryId = request.CategoryId;
        p.IsFeatured = request.IsFeatured;

        await _db.SaveChangesAsync();

        var updated = await _db.Products.AsNoTracking().Include(x => x.Category).FirstAsync(x => x.Id == id);
        return Ok(new ProductDetailDto(
            updated.Id,
            updated.Sku,
            updated.Name,
            updated.Brand,
            updated.Price,
            updated.Stock,
            updated.ImageUrl,
            updated.Description,
            updated.CategoryId,
            updated.Category != null ? updated.Category.Name : "",
            updated.IsFeatured,
            updated.CreatedAtUtc
        ));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p is null)
            return NotFound();

        _db.Products.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
