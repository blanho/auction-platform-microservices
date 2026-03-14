using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using AutoMapper;

namespace Auctions.Application.Mappings;

public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.AuctionCount, o => o.MapFrom(s => s.Items.Count));
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>();
    }
}
