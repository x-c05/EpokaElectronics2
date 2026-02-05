namespace EpokaElectronics.Api.Dtos;

public record CartItemRequest(int ProductId, int Quantity);

public record CreateOrderRequest(
    string ShippingName,
    string ShippingPhone,
    string ShippingAddressLine1,
    string? ShippingAddressLine2,
    string ShippingCity,
    string ShippingCountry,
    string? Notes,
    List<CartItemRequest> Items
);

public record OrderItemDto(
    int ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string ProductName,
    string ProductSku
);

public record OrderDto(
    int Id,
    DateTime CreatedAtUtc,
    decimal Subtotal,
    decimal Shipping,
    decimal Total,
    string Status,
    string ShippingName,
    string ShippingPhone,
    string ShippingAddressLine1,
    string? ShippingAddressLine2,
    string ShippingCity,
    string ShippingCountry,
    string? Notes,
    List<OrderItemDto> Items
);
