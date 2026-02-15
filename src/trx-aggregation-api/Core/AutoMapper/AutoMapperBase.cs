using aggregate_api.Core.AutoMapper.Profiles;

namespace aggregate_api.Core.AutoMapper;

public static class AutoMapperBase
{
    public static IServiceCollection InjectAutoMapperProfiles(this IServiceCollection services)
    {
        services
            .AddAutoMapper(typeof(AggregateProfileV1))
            .AddAutoMapper(typeof(TokenProfile));
        
        return services;
    }
}