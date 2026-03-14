using AutoMapper;
using Jobs.Domain.Entities;

namespace Jobs.Application.Mappings;

public class JobMappingProfile : Profile
{
    public JobMappingProfile()
    {
        CreateMap<Job, JobDto>();
        CreateMap<Job, JobSummaryDto>();
        CreateMap<JobItem, JobItemDto>();
    }
}
