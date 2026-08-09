namespace BookLoverECommerce.Web.Models.Products;

public sealed class ProductViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Category { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }
}