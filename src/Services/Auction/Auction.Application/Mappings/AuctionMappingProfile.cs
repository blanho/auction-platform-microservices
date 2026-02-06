using Auctions.Application.DTOs;
using Auctions.Application.DTOs.Auctions;
using Auctions.Domain.Entities;
using AutoMapper;

namespace Auctions.Application.Mappings;

public class AuctionMappingProfile : Profile
{
    public AuctionMappingProfile()
    {
        CreateMap<Auction, AuctionDto>()
            .IncludeMembers(x => x.Item)
            .ForMember(d => d.Seller, o => o.MapFrom(s => s.SellerUsername))
            .ForMember(d => d.Winner, o => o.MapFrom(s => s.WinnerUsername))
            .ForMember(d => d.Files, o => o.MapFrom(s => s.Item.Files))
            .ForMember(d => d.IsBuyNowAvailable, o => o.MapFrom(s => s.IsBuyNowAvailable));
        
        CreateMap<Item, AuctionDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.CategorySlug, o => o.MapFrom(s => s.Category != null ? s.Category.Slug : null))
            .ForMember(d => d.CategoryIcon, o => o.MapFrom(s => s.Category != null ? s.Category.Icon : null));

        CreateMap<CreateAuctionWithFileIdsDto, Auction>()
            .ForMember(d => d.Item, o => o.MapFrom(s => s));
        CreateMap<CreateAuctionWithFileIdsDto, Item>();

        CreateMap<AuctionDto, Auction>()
            .ForMember(d => d.Item, o => o.MapFrom(s => s));
        CreateMap<AuctionDto, Item>();

        CreateMap<Auction, AuctionCreatedEvent>()
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.UpdatedAt))
            .ForMember(d => d.AuctionEnd, o => o.MapFrom(s => s.AuctionEnd))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.ReservePrice, o => o.MapFrom(s => s.ReservePrice))
            .ForMember(d => d.Currency, o => o.MapFrom(s => s.Currency))
            .ForMember(d => d.SellerId, o => o.MapFrom(s => s.SellerId))
            .ForMember(d => d.SellerUsername, o => o.MapFrom(s => s.SellerUsername))
            .ForMember(d => d.WinnerId, o => o.MapFrom(s => s.WinnerId))
            .ForMember(d => d.WinnerUsername, o => o.MapFrom(s => s.WinnerUsername))
            .ForMember(d => d.Title, o => o.MapFrom(s => s.Item.Title))
            .ForMember(d => d.Description, o => o.MapFrom(s => s.Item.Description))
            .ForMember(d => d.Condition, o => o.MapFrom(s => s.Item.Condition))
            .ForMember(d => d.YearManufactured, o => o.MapFrom(s => s.Item.YearManufactured));

        CreateMap<MediaFile, AuctionFileDto>()
            .ForMember(d => d.FileId, o => o.MapFrom(s => s.FileId));
    }
}
