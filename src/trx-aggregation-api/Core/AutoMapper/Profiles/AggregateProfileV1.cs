using aggregate_api.Application.Dtos;
using aggregate_api.Application.Domain.Enums;
using aggregate_api.Application.Domain.Requests.v1;
using AutoMapper;
using aggregate_api.Application.Domain.Models;

namespace aggregate_api.Core.AutoMapper.Profiles;

public class AggregateProfileV1 : Profile
{
    public AggregateProfileV1()
    {
        CreateMap<SendAggregateRequest, CustomerAggregationCommand>()
            .ForMember(dest => dest.CustomerIds,
                opt => opt.MapFrom(src => src.CustomerIds))
            .ForMember(dest => dest.CorrelationId,
                opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.EventTriggerDate,
                opt => opt.MapFrom(_ => DateTime.UtcNow.ToString("o")))
            .ForMember(dest => dest.FromDate,
                opt => opt.MapFrom(src => src.FromDate))
            .ForMember(dest => dest.ToDate,
                opt => opt.MapFrom(src => src.ToDate))
            .ForMember(dest => dest.SourceSystem,
                opt => opt.MapFrom(src => src.SourceSystem));

    }
}