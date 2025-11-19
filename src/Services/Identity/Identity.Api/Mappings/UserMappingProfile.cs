using AutoMapper;
using Identity.Api.DTOs.External;
using Identity.Api.DTOs.Profile;
using Identity.Api.DTOs.Users;
using Identity.Api.Models;

namespace Identity.Api.Mappings;

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
