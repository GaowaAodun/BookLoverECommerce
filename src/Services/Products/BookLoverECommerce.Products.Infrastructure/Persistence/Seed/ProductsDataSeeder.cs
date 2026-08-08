using BookLoverECommerce.Products.Domain.Entities;
using BookLoverECommerce.Products.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BookLoverECommerce.Products.Infrastructure.Persistence.Seed;

public static class ProductsDataSeeder
{
    private const string SeedAdminUserId = "system-seed";

    public static async Task SeedAsync(
        ProductsDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        await SeedCategoriesAsync(
            dbContext,
            cancellationToken);

        await SeedProductsAsync(
            dbContext,
            cancellationToken);
    }

    private static async Task SeedCategoriesAsync(
        ProductsDbContext dbContext,
        CancellationToken cancellationToken)
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

    private static async Task SeedProductsAsync(
        ProductsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        var categories = await dbContext.Categories
            .ToDictionaryAsync(
                category => category.Name,
                category => category.Id,
                cancellationToken);

        var products = new[]
        {
            new Product(
                name: "The Great Gatsby",
                description:
                    "A classic novel by F. Scott Fitzgerald.",
                sku: "BOOK-GATSBY-001",
                price: 19.99m,
                stockQuantity: 50,
                categoryId: categories["Printed Books"],
                productType: ProductType.PrintedBook,
                createdByUserId: SeedAdminUserId,
                brand: "Scribner",
                thumbnailUrl:
                    "https://example.com/images/great-gatsby.jpg"),

            new Product(
                name: "Book Lover Cotton T-Shirt",
                description:
                    "A comfortable cotton T-shirt designed for book lovers.",
                sku: "CLOTHING-TSHIRT-001",
                price: 24.99m,
                stockQuantity: 30,
                categoryId: categories["Clothing"],
                productType: ProductType.Clothing,
                createdByUserId: SeedAdminUserId,
                brand: "BookLover",
                thumbnailUrl:
                    "https://example.com/images/book-tshirt.jpg"),

            new Product(
                name: "Children's Alphabet Puzzle",
                description:
                    "An educational wooden alphabet puzzle for children.",
                sku: "TOY-PUZZLE-001",
                price: 16.50m,
                stockQuantity: 25,
                categoryId: categories["Toys"],
                productType: ProductType.Toy,
                createdByUserId: SeedAdminUserId,
                brand: "LearnAndPlay",
                thumbnailUrl:
                    "https://example.com/images/alphabet-puzzle.jpg"),

            new Product(
                name: "Adjustable Reading Light",
                description:
                    "A rechargeable LED reading light with adjustable brightness.",
                sku: "ELECTRONICS-LIGHT-001",
                price: 29.99m,
                stockQuantity: 40,
                categoryId: categories["Electronics"],
                productType: ProductType.Electronics,
                createdByUserId: SeedAdminUserId,
                brand: "BrightRead",
                thumbnailUrl:
                    "https://example.com/images/reading-light.jpg"),

            new Product(
                name: "Literary Quote Mug",
                description:
                    "A ceramic mug featuring an inspirational literary quote.",
                sku: "GIFT-MUG-001",
                price: 14.99m,
                stockQuantity: 0,
                categoryId: categories["Gifts"],
                productType: ProductType.Gift,
                createdByUserId: SeedAdminUserId,
                brand: "BookLover",
                thumbnailUrl:
                    "https://example.com/images/literary-mug.jpg")
        };

        // Constructors create products with Draft status.
        // Publish them so they can appear in the storefront.
        foreach (var product in products)
        {
            product.Publish();
        }

        await dbContext.Products.AddRangeAsync(
            products,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}