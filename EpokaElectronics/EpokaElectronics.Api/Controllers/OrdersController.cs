using System.Security.Claims;
using EpokaElectronics.Api.Data;
using EpokaElectronics.Api.Dtos;
using EpokaElectronics.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EpokaElectronics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public OrdersController(ApplicationDbContext db)
    {
        _db = db;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderRequest request)
    {
        if (request.Items is null || request.Items.Count == 0)
            return BadRequest(new { message = "Cart is empty." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        if (products.Count != productIds.Count)
            return BadRequest(new { message = "Some products were not found." });

        foreach (var item in request.Items)
        {
            var prod = products.First(p => p.Id == item.ProductId);
            if (item.Quantity <= 0)
                return BadRequest(new { message = "Invalid quantity." });
            if (prod.Stock < item.Quantity)
                return BadRequest(new { message = $"Not enough stock for {prod.Name}." });
        }

        var subtotal = request.Items.Sum(i =>
        {
            var prod = products.First(p => p.Id == i.ProductId);
            return prod.Price * i.Quantity;
        });

        var shipping = subtotal >= 150 ? 0 : 5;
        var total = subtotal + shipping;

        var order = new Order
        {
            UserId = userId,
            Subtotal = subtotal,
            Shipping = shipping,
            Total = total,
            Status = "Pending",
            ShippingName = request.ShippingName.Trim(),
            ShippingPhone = request.ShippingPhone.Trim(),
            ShippingAddressLine1 = request.ShippingAddressLine1.Trim(),
            ShippingAddressLine2 = string.IsNullOrWhiteSpace(request.ShippingAddressLine2) ? null : request.ShippingAddressLine2.Trim(),
            ShippingCity = request.ShippingCity.Trim(),
            ShippingCountry = request.ShippingCountry.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            var prod = products.First(p => p.Id == item.ProductId);
            prod.Stock -= item.Quantity;

            order.Items.Add(new OrderItem
            {
                ProductId = prod.Id,
                Quantity = item.Quantity,
                UnitPrice = prod.Price,
                LineTotal = prod.Price * item.Quantity,
                ProductName = prod.Name,
                ProductSku = prod.Sku
            });
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return Ok(MapOrder(order));
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<List<OrderDto>>> Mine()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var orders = await _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync();

        return Ok(orders.Select(MapOrder).ToList());
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> All()
    {
        var orders = await _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync();

        return Ok(orders.Select(MapOrder).ToList());
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order is null)
            return NotFound();

        var normalized = status.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return BadRequest();

        order.Status = normalized;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static OrderDto MapOrder(Order o)
    {
        return new OrderDto(
            o.Id,
            o.CreatedAtUtc,
            o.Subtotal,
            o.Shipping,
            o.Total,
            o.Status,
            o.ShippingName,
            o.ShippingPhone,
            o.ShippingAddressLine1,
            o.ShippingAddressLine2,
            o.ShippingCity,
            o.ShippingCountry,
            o.Notes,
            o.Items.Select(i => new OrderItemDto(i.ProductId, i.Quantity, i.UnitPrice, i.LineTotal, i.ProductName, i.ProductSku)).ToList()
        );
    }
}
