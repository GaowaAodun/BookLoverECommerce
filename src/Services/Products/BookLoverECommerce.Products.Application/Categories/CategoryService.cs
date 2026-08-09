using BookLoverECommerce.Products.Application.Abstractions;
using BookLoverECommerce.Products.Application.DTOs;
using BookLoverECommerce.Products.Application.Exceptions;
using BookLoverECommerce.Products.Domain.Entities;

namespace BookLoverECommerce.Products.Application.Categories;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(
        ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var categories =
            await _categoryRepository.GetActiveAsync(
                cancellationToken);

        return categories
            .Select(MapToDto)
            .ToArray();
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllForAdminAsync(
        CancellationToken cancellationToken = default)
    {
        var categories =
            await _categoryRepository.GetAllAsync(
                cancellationToken);

        return categories
            .Select(MapToDto)
            .ToArray();
    }

    public async Task<CategoryDto> GetByIdAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        var category =
            await GetCategoryOrThrowAsync(
                categoryId,
                cancellationToken);

        return MapToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(
        CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = command.Name.Trim();
        var normalizedDescription =
            NormalizeOptionalText(command.Description);

        await EnsureUniqueNameAsync(
            normalizedName,
            excludingCategoryId: null,
            cancellationToken);

        if (command.ParentCategoryId.HasValue)
        {
            await GetValidRootParentAsync(
                command.ParentCategoryId.Value,
                cancellationToken);
        }

        var category = new Category(
            normalizedName,
            normalizedDescription,
            command.DisplayOrder,
            command.ParentCategoryId);

        await _categoryRepository.AddAsync(
            category,
            cancellationToken);

        await _categoryRepository.SaveChangesAsync(
            cancellationToken);

        return MapToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(
    int categoryId,
    UpdateCategoryCommand command,
    CancellationToken cancellationToken = default)
    {
        var category =
            await GetCategoryOrThrowAsync(
                categoryId,
                cancellationToken);

        var normalizedName = command.Name.Trim();
        var normalizedDescription =
            NormalizeOptionalText(command.Description);

        await EnsureUniqueNameAsync(
            normalizedName,
            categoryId,
            cancellationToken);

        await ValidateCategoryTierChangeAsync(
            category,
            command.ParentCategoryId,
            cancellationToken);

        if (command.ParentCategoryId.HasValue)
        {
            if (command.ParentCategoryId.Value == categoryId)
            {
                throw new InvalidCategoryHierarchyException(
                    "A category cannot be its own parent.");
            }

            await GetValidRootParentAsync(
                command.ParentCategoryId.Value,
                cancellationToken);
        }

        category.Update(
            normalizedName,
            normalizedDescription,
            command.DisplayOrder,
            command.ParentCategoryId);

        await _categoryRepository.SaveChangesAsync(
            cancellationToken);

        return MapToDto(category);
    }

    public async Task ActivateAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        var category =
            await GetCategoryOrThrowAsync(
                categoryId,
                cancellationToken);

        if (category.ParentCategoryId.HasValue)
        {
            var parentCategory =
                await GetCategoryOrThrowAsync(
                    category.ParentCategoryId.Value,
                    cancellationToken);

            if (parentCategory.ParentCategoryId.HasValue)
            {
                throw new InvalidCategoryHierarchyException(
                    "The category cannot be activated because its parent " +
                    "is not a root category.");
            }

            if (!parentCategory.IsActive)
            {
                throw new InvalidCategoryHierarchyException(
                    "The category cannot be activated because its root " +
                    "category is inactive.");
            }
        }

        category.Activate();

        await _categoryRepository.SaveChangesAsync(
            cancellationToken);
    }

    public async Task DeactivateAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        var category =
            await GetCategoryOrThrowAsync(
                categoryId,
                cancellationToken);

        var isRootCategory =
            !category.ParentCategoryId.HasValue;

        if (isRootCategory)
        {
            var activeCategories =
                await _categoryRepository.GetActiveAsync(
                    cancellationToken);

            var hasActiveChildren =
                activeCategories.Any(
                    child =>
                        child.ParentCategoryId == categoryId);

            if (hasActiveChildren)
            {
                throw new InvalidCategoryHierarchyException(
                    "The root category cannot be deactivated while it " +
                    "has active child categories. Deactivate or move its " +
                    "active child categories first.");
            }
        }

        category.Deactivate();

        await _categoryRepository.SaveChangesAsync(
            cancellationToken);
    }

    public async Task DeleteAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        var category =
            await GetCategoryOrThrowAsync(
                categoryId,
                cancellationToken);

        var hasProducts =
            await _categoryRepository.HasProductsAsync(
                categoryId,
                cancellationToken);

        if (hasProducts)
        {
            throw new CategoryInUseException(
                "The category cannot be deleted because it contains products.");
        }

        var hasChildren =
            await _categoryRepository.HasChildCategoriesAsync(
                categoryId,
                cancellationToken);

        if (hasChildren)
        {
            throw new CategoryInUseException(
                "The category cannot be deleted because it has child categories.");
        }

        _categoryRepository.Remove(category);

        await _categoryRepository.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<Category> GetCategoryOrThrowAsync(
        int categoryId,
        CancellationToken cancellationToken)
    {
        return await _categoryRepository.GetByIdAsync(
            categoryId,
            cancellationToken)
            ?? throw new CategoryNotFoundException(categoryId);
    }

    private async Task<Category> GetValidRootParentAsync(
        int parentCategoryId,
        CancellationToken cancellationToken)
    {
        var parentCategory =
            await _categoryRepository.GetByIdAsync(
                parentCategoryId,
                cancellationToken)
            ?? throw new CategoryNotFoundException(
                parentCategoryId);

        if (parentCategory.ParentCategoryId.HasValue)
        {
            throw new InvalidCategoryHierarchyException(
                "A child category can only belong to a root category.");
        }

        if (!parentCategory.IsActive)
        {
            throw new InvalidCategoryHierarchyException(
                "An inactive root category cannot be selected as a parent.");
        }

        return parentCategory;
    }

    private async Task EnsureUniqueNameAsync(
        string name,
        int? excludingCategoryId,
        CancellationToken cancellationToken)
    {
        var nameExists =
            await _categoryRepository.NameExistsAsync(
                name,
                excludingCategoryId,
                cancellationToken);

        if (nameExists)
        {
            throw new DuplicateCategoryNameException(name);
        }
    }

    private async Task ValidateCategoryTierChangeAsync(
    Category existingCategory,
    int? requestedParentCategoryId,
    CancellationToken cancellationToken)
    {
        var isExistingRoot =
            !existingCategory.ParentCategoryId.HasValue;

        var isRequestedRoot =
            !requestedParentCategoryId.HasValue;

        // A root category must remain a root category.
        if (isExistingRoot && !isRequestedRoot)
        {
            throw new InvalidCategoryHierarchyException(
                "A root category cannot be changed into a child category.");
        }

        // A child category may become a root category only when
        // it does not have any child categories.
        if (!isExistingRoot && isRequestedRoot)
        {
            var hasChildCategories =
                await _categoryRepository.HasChildCategoriesAsync(
                    existingCategory.Id,
                    cancellationToken);

            if (hasChildCategories)
            {
                throw new InvalidCategoryHierarchyException(
                    "This child category cannot be changed into a root category " +
                    "because it has child categories. Move or delete its child " +
                    "categories first.");
            }
        }
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static CategoryDto MapToDto(
        Category category)
    {
        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.DisplayOrder,
            category.IsActive,
            category.ParentCategoryId);
    }
}