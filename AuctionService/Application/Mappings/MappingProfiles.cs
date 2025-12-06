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
                .IncludeMembers(x => x.Item);
            CreateMap<Item, AuctionDto>();

            CreateMap<CreateAuctionDto, Auction>()
                .ForMember(d => d.Item, o => o.MapFrom(s => s));
            CreateMap<CreateAuctionDto, Item>();

            CreateMap<AuctionDto, Auction>()
                .ForMember(d => d.Item, o => o.MapFrom(s => s));
            CreateMap<AuctionDto, Item>();

            CreateMap<Auction, AuctionCreatedEvent>()
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.DateTime))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.UpdatedAt.HasValue ? s.UpdatedAt.Value.DateTime : (DateTime?)null))
                .ForMember(d => d.AuctionEnd, o => o.MapFrom(s => s.AuctionEnd.DateTime))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.ReservePrice, o => o.MapFrom(s => s.ReversePrice))
                .ForMember(d => d.Title, o => o.MapFrom(s => s.Item.Title))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Item.Description))
                .ForMember(d => d.Make, o => o.MapFrom(s => s.Item.Make))
                .ForMember(d => d.Model, o => o.MapFrom(s => s.Item.Model))
                .ForMember(d => d.Year, o => o.MapFrom(s => s.Item.Year))
                .ForMember(d => d.Color, o => o.MapFrom(s => s.Item.Color))
                .ForMember(d => d.Mileage, o => o.MapFrom(s => s.Item.Mileage));

            CreateMap<AuctionFileInfo, AuctionFileDto>();
        }
    }
}
