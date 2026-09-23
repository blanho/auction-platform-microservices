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
        public static Error HasChildren => Error.Create("Category.HasChildren", "Reassign or remove child categories before deleting or deactivating their parent");
        public static Error ParentInactive => Error.Create("Category.ParentInactive", "An active category must have an active parent");
        public static Error ParentNotFound => Error.Create("Category.ParentNotFound", "Parent category not found");
        public static Error CannotBeOwnParent => Error.Create("Category.CannotBeOwnParent", "A category cannot be its own parent");
    }
}
