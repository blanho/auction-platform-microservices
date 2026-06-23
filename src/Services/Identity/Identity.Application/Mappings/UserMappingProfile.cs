using AutoMapper;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.Profile;
using Identity.Application.DTOs.Users;
using Identity.Domain.Entities;

namespace Identity.Application.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(d => d.Username, o => o.MapFrom(s => s.UserName))
            .ForMember(d => d.Roles, o => o.Ignore());

        CreateMap<ApplicationUser, AdminUserDto>()
            .ForMember(d => d.Username, o => o.MapFrom(s => s.UserName))
            .ForMember(d => d.Roles, o => o.Ignore());

        CreateMap<ApplicationUser, UserProfileDto>()
            .ForMember(d => d.Username, o => o.MapFrom(s => s.UserName))
            .ForMember(d => d.Roles, o => o.Ignore());

        CreateMap<ApplicationUser, ExternalLoginTokenResponse>()
            .ForMember(d => d.UserId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Username, o => o.MapFrom(s => s.UserName))
            .ForMember(d => d.Roles, o => o.Ignore());
    }
}
