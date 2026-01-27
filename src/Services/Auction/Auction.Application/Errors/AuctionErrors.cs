using BuildingBlocks.Application.Abstractions;

namespace Auctions.Application.Errors;

public static class AuctionErrors
{
    public static class Auction
    {
        public static Error NotFound => Error.Create("Auction.NotFound", "Auction not found");
        public static Error NotFoundById(Guid id) => Error.Create("Auction.NotFound", $"Auction with ID {id} was not found");
        public static Error FetchFailed(string reason) => Error.Create("Auction.FetchFailed", $"Failed to fetch auction: {reason}");
        public static Error InvalidStatus(string currentStatus) => Error.Create("Auction.InvalidStatus", $"Cannot deactivate auction in {currentStatus} status. Only Draft or Scheduled auctions can be deactivated.");
        public static Error DeactivationFailed(string reason) => Error.Create("Auction.DeactivationFailed", reason);
        public static Error ExportFailed(string reason) => Error.Create("Auction.ExportFailed", reason);
        public static Error BulkUpdateFailed(string reason) => Error.Create("Auction.BulkUpdateFailed", $"Failed to bulk update auctions: {reason}");
    }

    public static class BuyNow
    {
        public static Error Conflict => Error.Create("BuyNow.Conflict", "Another buyer is currently processing this purchase. Please try again.");
        public static Error ConflictPurchased => Error.Create("BuyNow.Conflict", "This item was just purchased by someone else. Please try another auction.");
        public static Error NotAvailable => Error.Create("BuyNow.NotAvailable", "Buy Now is not available for this auction");
        public static Error OwnAuction => Error.Create("BuyNow.OwnAuction", "You cannot buy your own auction");
        public static Error AuctionNotLive => Error.Create("BuyNow.AuctionNotLive", "This auction is no longer active");
        public static Error Failed(string reason) => Error.Create("BuyNow.Failed", $"Failed to process Buy Now: {reason}");
    }

    public static class Bookmark
    {
        public static Error NotFound => Error.Create("Bookmark.NotFound", "Item not found in watchlist");
        public static Error AlreadyExists => Error.Create("Bookmark.AlreadyExists", "Auction is already in your watchlist");
    }

    public static class Brand
    {
        public static Error NotFound => Error.Create("Brand.NotFound", "Brand not found");
        public static Error NotFoundById(Guid id) => Error.Create("Brand.NotFound", $"Brand with ID '{id}' was not found");
        public static Error SlugExists(string slug) => Error.Create("Brand.SlugExists", $"A brand with slug '{slug}' already exists");
        public static Error DeleteFailed(string reason) => Error.Create("Brand.DeleteFailed", $"Failed to delete brand: {reason}");
        public static Error UpdateFailed(string reason) => Error.Create("Brand.UpdateFailed", $"Failed to update brand: {reason}");
        public static Error FetchError(string reason) => Error.Create("Brand.FetchError", $"Error fetching brand: {reason}");
    }

    public static class Category
    {
        public static Error NotFound => Error.Create("Category.NotFound", "Category not found");
        public static Error NotFoundById(Guid id) => Error.Create("Category.NotFound", $"Category with ID {id} not found");
        public static Error SlugExists(string slug) => Error.Create("Category.SlugExists", $"A category with slug '{slug}' already exists");
        public static Error SelfParent => Error.Create("Category.SelfParent", "A category cannot be its own parent");
        public static Error HasItems => Error.Create("Category.HasItems", "Cannot delete category that has associated items. Please reassign or delete the items first.");
        public static Error CreateFailed(string reason) => Error.Create("Category.CreateFailed", $"Failed to create category: {reason}");
        public static Error UpdateFailed(string reason) => Error.Create("Category.UpdateFailed", $"Failed to update category: {reason}");
        public static Error DeleteFailed(string reason) => Error.Create("Category.DeleteFailed", $"Failed to delete category: {reason}");
        public static Error ImportFailed(string reason) => Error.Create("Category.ImportFailed", $"Failed to import categories: {reason}");
        public static Error FetchError(string reason) => Error.Create("Categories.FetchError", $"Error fetching categories: {reason}");
    }

    public static class Review
    {
        public static Error NotFound => Error.Create("Review.NotFound", "Review not found");
        public static Error NotFoundById(Guid id) => Error.Create("Review.NotFound", $"Review with ID {id} not found");
        public static Error AlreadyExists => Error.Create("Review.AlreadyExists", "Review already exists for this auction");
        public static Error InvalidRating => Error.Create("Review.InvalidRating", "Rating must be between 1 and 5");
        public static Error Forbidden => Error.Create("Review.Forbidden", "Only the reviewed seller can respond to this review");
        public static Error AlreadyResponded => Error.Create("Review.AlreadyResponded", "Seller has already responded to this review");
    }

    public static class Analytics
    {
        public static Error Error => Error.Create("Analytics.Error", "Failed to retrieve seller analytics");
    }

    public static class Dashboard
    {
        public static Error Error => Error.Create("Dashboard.Error", "Failed to retrieve dashboard statistics");
    }
}
