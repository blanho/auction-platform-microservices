using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using AutoMapper;

namespace Auctions.Application.Mappings;

public class ReviewMappingProfile : Profile
{
    public ReviewMappingProfile()
    {
        CreateMap<Review, ReviewDto>();
    }
}
