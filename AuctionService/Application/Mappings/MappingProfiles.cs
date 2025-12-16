using AuctionService.Application.DTOs;
using AuctionService.Domain.Entities;
using AutoMapper;
using Common.Messaging.Events;

namespace AuctionService.Application.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
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

            CreateMap<CreateAuctionDto, Auction>()
                .ForMember(d => d.Item, o => o.MapFrom(s => s));
            CreateMap<CreateAuctionDto, Item>();

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

            CreateMap<ItemFileInfo, AuctionFileDto>();

            CreateMap<Category, CategoryDto>()
                .ForMember(d => d.AuctionCount, o => o.MapFrom(s => s.Items.Count));
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();

            CreateMap<Review, ReviewDto>();

            CreateMap<Brand, BrandDto>();
            CreateMap<CreateBrandDto, Brand>();
            CreateMap<UpdateBrandDto, Brand>();

            CreateMap<FlashSale, FlashSaleDto>();
            CreateMap<FlashSaleItem, FlashSaleItemDto>();
            CreateMap<CreateFlashSaleDto, FlashSale>();
            CreateMap<UpdateFlashSaleDto, FlashSale>();
        }
    }
}
