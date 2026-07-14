namespace BookLoverECommerce.Products.Domain.Entities;

public class Category
{
    private Category()
    {
    }

    public Category(
        string name,
        string? description = null,
        int displayOrder = 0,
        int? parentCategoryId = null)
    {
        SetName(name);
        SetDescription(description);
        SetDisplayOrder(displayOrder);

        ParentCategoryId = parentCategoryId;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsActive { get; private set; }

    public int? ParentCategoryId { get; private set; }

    public Category? ParentCategory { get; private set; }

    public ICollection<Category> ChildCategories { get; private set; }
        = new List<Category>();

    public ICollection<Product> Products { get; private set; }
        = new List<Product>();

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public void Update(
        string name,
        string? description,
        int displayOrder,
        int? parentCategoryId)
    {
        if (parentCategoryId == Id)
        {
            throw new InvalidOperationException(
                "A category cannot be its own parent.");
        }

        SetName(name);
        SetDescription(description);
        SetDisplayOrder(displayOrder);

        ParentCategoryId = parentCategoryId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Category name is required.",
                nameof(name));
        }

        if (name.Length > 100)
        {
            throw new ArgumentException(
                "Category name cannot exceed 100 characters.",
                nameof(name));
        }

        Name = name.Trim();
    }

    private void SetDescription(string? description)
    {
        if (description is not null && description.Length > 500)
        {
            throw new ArgumentException(
                "Category description cannot exceed 500 characters.",
                nameof(description));
        }

        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    private void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(displayOrder),
                "Display order cannot be negative.");
        }

        DisplayOrder = displayOrder;
    }
}