namespace EpokaElectronics.Api.Entities;

public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public decimal Subtotal { get; set; }
    public decimal Shipping { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "Pending";
    public string ShippingName { get; set; } = "";
    public string ShippingPhone { get; set; } = "";
    public string ShippingAddressLine1 { get; set; } = "";
    public string? ShippingAddressLine2 { get; set; }
    public string ShippingCity { get; set; } = "";
    public string ShippingCountry { get; set; } = "";
    public string? Notes { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
