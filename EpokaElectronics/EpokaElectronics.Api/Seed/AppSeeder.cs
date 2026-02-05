using EpokaElectronics.Api.Data;
using EpokaElectronics.Api.Entities;
using EpokaElectronics.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EpokaElectronics.Api.Seed;

public static class AppSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var db = sp.GetRequiredService<ApplicationDbContext>();

        var roles = new[] { "Admin", "Customer" };
        foreach (var r in roles)
        {
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole(r));
        }

        var adminEmail = "admin@epoka-electronics.com";
        var adminUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                Email = adminEmail,
                UserName = adminEmail,
                FullName = "Epoka Admin",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        if (!await db.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category { Name = "Smartphones" },
                new Category { Name = "Laptops" },
                new Category { Name = "Headphones" },
                new Category { Name = "Smart Home" },
                new Category { Name = "Gaming" },
                new Category { Name = "TV & Audio" }
            };
            db.Categories.AddRange(categories);
            await db.SaveChangesAsync();
        }

        if (!await db.Products.AnyAsync())
        {
            var cats = await db.Categories.AsNoTracking().ToListAsync();
            int Cat(string name) => cats.First(c => c.Name == name).Id;

            var products = new List<Product>
            {
                new Product { Sku="EPO-SM-001", Name="Epoka Nova X", Brand="Epoka", Price=699, Stock=22, CategoryId=Cat("Smartphones"), IsFeatured=true, ImageUrl="/assets/products/phone-1.svg", Description="6.7-inch OLED, 120Hz, 256GB storage, 5G, pro-grade camera system." },
                new Product { Sku="EPO-SM-002", Name="ZenPhone Pro 12", Brand="Zen", Price=799, Stock=15, CategoryId=Cat("Smartphones"), IsFeatured=false, ImageUrl="/assets/products/phone-2.svg", Description="Flagship performance with a refined camera pipeline and long battery life." },
                new Product { Sku="EPO-LT-010", Name="AeroBook 14", Brand="Aero", Price=1099, Stock=10, CategoryId=Cat("Laptops"), IsFeatured=true, ImageUrl="/assets/products/laptop-1.svg", Description="14-inch ultrabook, Intel i7, 16GB RAM, 512GB SSD, lightweight aluminum chassis." },
                new Product { Sku="EPO-LT-011", Name="StudioLine 16", Brand="Studio", Price=1499, Stock=6, CategoryId=Cat("Laptops"), IsFeatured=false, ImageUrl="/assets/products/laptop-2.svg", Description="16-inch creator laptop with color-accurate display and powerful GPU." },
                new Product { Sku="EPO-HP-100", Name="Pulse ANC Headphones", Brand="Pulse", Price=199, Stock=40, CategoryId=Cat("Headphones"), IsFeatured=true, ImageUrl="/assets/products/headphones-1.svg", Description="Active noise cancellation, 30-hour battery, premium comfort fit." },
                new Product { Sku="EPO-SH-220", Name="HomeHub Mini", Brand="HomeHub", Price=49, Stock=60, CategoryId=Cat("Smart Home"), IsFeatured=false, ImageUrl="/assets/products/smarthome-1.svg", Description="Compact smart speaker with voice assistant integration and multi-room audio." },
                new Product { Sku="EPO-GM-300", Name="Neon Controller", Brand="Neon", Price=59, Stock=50, CategoryId=Cat("Gaming"), IsFeatured=false, ImageUrl="/assets/products/gaming-1.svg", Description="Low-latency wireless controller with adaptive triggers and ergonomic grip." },
                new Product { Sku="EPO-TV-500", Name="Aurora 55-inch 4K TV", Brand="Aurora", Price=649, Stock=8, CategoryId=Cat("TV & Audio"), IsFeatured=true, ImageUrl="/assets/products/tv-1.svg", Description="4K HDR, smart apps, cinematic color, ultra-slim design." }
            };

            db.Products.AddRange(products);
            await db.SaveChangesAsync();
        }
    }
}
