using BookLoverECommerce.Products.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookLoverECommerce.Products.Infrastructure.Persistence.Seed;

public static class ProductsDataSeeder
{
    public static async Task SeedAsync(
        ProductsDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        var categories = new[]
        {
            new Category(
                name: "Printed Books",
                description: "Printed books in different genres.",
                displayOrder: 1),

            new Category(
                name: "Clothing",
                description: "Clothing and apparel.",
                displayOrder: 2),

            new Category(
                name: "Toys",
                description: "Toys and educational products.",
                displayOrder: 3),

            new Category(
                name: "Electronics",
                description: "Electronic devices and accessories.",
                displayOrder: 4),

            new Category(
                name: "Gifts",
                description: "Gift products and merchandise.",
                displayOrder: 5)
        };

        await dbContext.Categories.AddRangeAsync(
            categories,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}