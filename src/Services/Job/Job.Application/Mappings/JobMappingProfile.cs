using AutoMapper;
using BuildingBlocks.Application.Localization;
using Jobs.Domain.Entities;

namespace Jobs.Application.Mappings;

public class JobMappingProfile : Profile
{
    public JobMappingProfile()
    {
        CreateMap<Job, JobDto>()
            .ForMember(dto => dto.ErrorMessage, opt => opt.MapFrom(job => UserMessageLocalizer.TranslateOptional(job.ErrorMessage)));
        CreateMap<Job, JobSummaryDto>();
        CreateMap<JobItem, JobItemDto>()
            .ForMember(dto => dto.ErrorMessage, opt => opt.MapFrom(item => UserMessageLocalizer.TranslateOptional(item.ErrorMessage)));
    }
}
