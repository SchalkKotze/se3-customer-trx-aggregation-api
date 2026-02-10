using aggregate_api.Application.Dtos;
using aggregate_api.Core.FluentValidators.Services;
using aggregate_api.Core.FluentValidators.Services.Contracts;
using aggregate_api.Core.FluentValidators.TokenValidators;
using FluentValidation;

namespace aggregate_api.Core.FluentValidators;

public static class FluentValidatorBase
{
    public static IServiceCollection InjectFluentValidators(this IServiceCollection services)
    {
        services
            .AddTransient<IFluentValidationService, FluentValidationService>()
            .AddTransient<IValidator<CustomerAggregationCommand>, AggregateDtoValidator>()
            .AddTransient<IValidator<GenerateTokenDto>, GenerateTokenValidator>();
        
        return services;
    }
}