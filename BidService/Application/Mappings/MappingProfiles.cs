using AutoMapper;
using BidService.Application.DTOs;
using BidService.Domain.Entities;
using Common.Messaging.Events;

namespace BidService.Application.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Bid, BidDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

            CreateMap<PlaceBidDto, Bid>()
                .ForMember(d => d.BidTime, o => o.MapFrom(s => DateTime.UtcNow))
                .ForMember(d => d.Status, o => o.Ignore());

            CreateMap<Bid, BidPlacedEvent>()
                .ForMember(d => d.BidAmount, o => o.MapFrom(s => s.Amount))
                .ForMember(d => d.BidStatus, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.BidTime, o => o.MapFrom(s => s.BidTime));
        }
    }
}
