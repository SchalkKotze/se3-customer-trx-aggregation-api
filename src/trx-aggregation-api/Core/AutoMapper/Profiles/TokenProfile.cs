using aggregate_api.Application.Dtos;
using aggregate_api.Application.Domain.Requests.v1;
using AutoMapper;

namespace aggregate_api.Core.AutoMapper.Profiles;

public class TokenProfile : Profile
{
    public TokenProfile()
    {
        CreateMap<GenerateTokenRequest, GenerateTokenDto>()
            .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.ClientId))
            .ForMember(dest => dest.ClientSecret, opt => opt.MapFrom(src => src.ClientSecret));
    }
}