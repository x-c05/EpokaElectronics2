using EpokaElectronics.Api.Entities;
using EpokaElectronics.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EpokaElectronics.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<Product>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(140).IsRequired();
            e.Property(x => x.Brand).HasMaxLength(80);
            e.Property(x => x.Sku).HasMaxLength(40).IsRequired();
            e.HasIndex(x => x.Sku).IsUnique();
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Property(x => x.ImageUrl).HasMaxLength(400);
            e.Property(x => x.Description).HasMaxLength(4000);
            e.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId);
        });

        builder.Entity<Order>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Subtotal).HasPrecision(18, 2);
            e.Property(x => x.Shipping).HasPrecision(18, 2);
            e.Property(x => x.Total).HasPrecision(18, 2);
            e.Property(x => x.ShippingName).HasMaxLength(140).IsRequired();
            e.Property(x => x.ShippingPhone).HasMaxLength(50).IsRequired();
            e.Property(x => x.ShippingAddressLine1).HasMaxLength(200).IsRequired();
            e.Property(x => x.ShippingCity).HasMaxLength(100).IsRequired();
            e.Property(x => x.ShippingCountry).HasMaxLength(100).IsRequired();
            e.HasMany(x => x.Items).WithOne(x => x.Order).HasForeignKey(x => x.OrderId);
        });

        builder.Entity<OrderItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.LineTotal).HasPrecision(18, 2);
            e.Property(x => x.ProductName).HasMaxLength(140).IsRequired();
            e.Property(x => x.ProductSku).HasMaxLength(40).IsRequired();
        });
    }
}
