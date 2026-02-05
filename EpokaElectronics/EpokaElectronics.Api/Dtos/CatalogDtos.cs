namespace EpokaElectronics.Api.Dtos;

public record CategoryDto(int Id, string Name);

public record ProductListDto(
    int Id,
    string Sku,
    string Name,
    string? Brand,
    decimal Price,
    int Stock,
    string? ImageUrl,
    int CategoryId,
    string CategoryName,
    bool IsFeatured
);

public record ProductDetailDto(
    int Id,
    string Sku,
    string Name,
    string? Brand,
    decimal Price,
    int Stock,
    string? ImageUrl,
    string? Description,
    int CategoryId,
    string CategoryName,
    bool IsFeatured,
    DateTime CreatedAtUtc
);

public record ProductUpsertRequest(
    string Sku,
    string Name,
    string? Brand,
    decimal Price,
    int Stock,
    string? ImageUrl,
    string? Description,
    int CategoryId,
    bool IsFeatured
);
