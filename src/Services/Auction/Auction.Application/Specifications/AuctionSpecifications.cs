using Auctions.Domain.Entities;
using BuildingBlocks.Domain.Enums;
namespace Auctions.Application.Specifications;

public class LiveAuctionsSpecification : BaseSpecification<Auction>
{
    public LiveAuctionsSpecification()
        : base(a => a.Status == Status.Live && !a.IsDeleted)
    {
        AddInclude(a => a.Item);
    }
}

public class EndingSoonSpecification : BaseSpecification<Auction>
{
    public EndingSoonSpecification(TimeSpan window)
        : base(a => a.Status == Status.Live
                 && !a.IsDeleted
                 && a.AuctionEnd <= DateTimeOffset.UtcNow.Add(window))
    {
        ApplyOrderBy(a => a.AuctionEnd);
        AddInclude(a => a.Item);
    }

    public EndingSoonSpecification() : this(TimeSpan.FromHours(1)) { }
}

public class FeaturedAuctionsSpecification : BaseSpecification<Auction>
{
    public FeaturedAuctionsSpecification(int? limit = null)
        : base(a => a.Status == Status.Live && a.IsFeatured && !a.IsDeleted)
    {
        ApplyOrderByDescending(a => a.CurrentHighBid ?? 0);
        AddInclude(a => a.Item);
        AddInclude("Item.Files");

        if (limit.HasValue)
            ApplyPaging(0, limit.Value);
    }
}

public class AuctionsBySellerSpecification : BaseSpecification<Auction>
{
    public AuctionsBySellerSpecification(Guid sellerId, bool includeDeleted = false)
        : base(a => a.SellerId == sellerId && (includeDeleted || !a.IsDeleted))
    {
        ApplyOrderByDescending(a => a.CreatedAt);
        AddInclude(a => a.Item);
    }
}

public class WonAuctionsSpecification : BaseSpecification<Auction>
{
    public WonAuctionsSpecification(Guid winnerId)
        : base(a => a.WinnerId == winnerId && a.Status == Status.Finished)
    {
        ApplyOrderByDescending(a => a.AuctionEnd);
        AddInclude(a => a.Item);
    }
}

public class FinishableAuctionsSpecification : BaseSpecification<Auction>
{
    public FinishableAuctionsSpecification()
        : base(a => a.Status == Status.Live
                 && !a.IsDeleted
                 && a.AuctionEnd <= DateTimeOffset.UtcNow)
    {
        AddInclude(a => a.Item);
    }
}

public class TrendingAuctionsSpecification : BaseSpecification<Auction>
{
    public TrendingAuctionsSpecification(int limit = 10)
        : base(a => a.Status == Status.Live
                 && !a.IsDeleted
                 && a.CurrentHighBid.HasValue)
    {
        ApplyOrderByDescending(a => a.CurrentHighBid!.Value);
        ApplyPaging(0, limit);
        AddInclude(a => a.Item);
        AddInclude("Item.Files");
    }
}

public class AuctionsByCategorySpecification : BaseSpecification<Auction>
{
    public AuctionsByCategorySpecification(string categorySlug)
        : base(a => a.Status == Status.Live
                 && !a.IsDeleted
                 && a.Item.Category != null
                 && a.Item.Category.Slug == categorySlug.ToLower())
    {
        ApplyOrderByDescending(a => a.AuctionEnd);
        AddInclude(a => a.Item);
        AddInclude("Item.Category");
        AddInclude("Item.Files");
    }
}
