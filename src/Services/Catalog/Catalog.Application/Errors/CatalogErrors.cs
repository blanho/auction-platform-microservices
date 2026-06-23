using BuildingBlocks.Application.Abstractions;

namespace Catalog.Application.Errors;

public static class CatalogErrors
{
    public static class Brand
    {
        public static Error NotFound => Error.Create("Brand.NotFound", "Brand not found");
        public static Error SlugAlreadyExists => Error.Create("Brand.SlugAlreadyExists", "A brand with this slug already exists");
    }

    public static class Category
    {
        public static Error NotFound => Error.Create("Category.NotFound", "Category not found");
        public static Error SlugAlreadyExists => Error.Create("Category.SlugAlreadyExists", "A category with this slug already exists");
        public static Error ParentNotFound => Error.Create("Category.ParentNotFound", "Parent category not found");
        public static Error CannotBeOwnParent => Error.Create("Category.CannotBeOwnParent", "A category cannot be its own parent");
    }
}
