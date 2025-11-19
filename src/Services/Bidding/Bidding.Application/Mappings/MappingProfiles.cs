namespace Bidding.Application.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Bid, BidDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

            CreateMap<PlaceBidDto, Bid>()
                .ForMember(d => d.BidTime, o => o.MapFrom(s => DateTimeOffset.UtcNow))
                .ForMember(d => d.Status, o => o.Ignore());

            CreateMap<AutoBid, AutoBidDto>();
        }
    }
}

