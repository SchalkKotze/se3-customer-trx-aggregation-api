using aggregate_api.Application.Domain.Constants;
using aggregate_api.Application.Domain.Models;
using aggregate_api.Core.FluentValidators.Services.Contracts;
using aggregate_api.Infrastructure.Contracts;
using FluentValidation;

namespace aggregate_api.Core.FluentValidators.Services;

public class FluentValidationService : IFluentValidationService
{
    private readonly ILoggingService _loggingService;
    
    public FluentValidationService(ILoggingService loggingService)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
    }
    
    public ResponseModel ValidateAggregateCommand<TDto>(TDto dto, IValidator<TDto> validator)
    {
        _loggingService.LogTrace(LoggingMessages.Executing("FluentValidationService", "ValidateAggregateDto"));

        var r = new ResponseModel();
        try
        {
            var validationResult = validator.Validate(dto);
       
        if (validationResult.Errors.Any())
        {
            foreach (var error in validationResult.Errors)
            {
                _loggingService.LogError(error.ErrorMessage, r);
            }
        }
        
        return r;
        }
        catch (Exception ex)
        {
            var a = 1;
            return r;
        }

    }

    public ResponseModel ValidateDtos<TDto>(IList<TDto> dtos, IValidator<TDto> validator)
    {
        throw new NotImplementedException();
    }

    public ResponseModel ValidateAggreateDto<TDto>(IList<TDto> dtos, IValidator<TDto> validator)
    {
        _loggingService.LogTrace(LoggingMessages.Executing("FluentValidationService", "ValidateAggregateDtos"));

        var r = new ResponseModel();
        
        foreach (var dto in dtos)
        {
            var validationResult = validator.Validate(dto);

            if (validationResult.Errors.Any())
            {
                foreach (var error in validationResult.Errors)
                {
                    _loggingService.LogError(error.ErrorMessage, r);
                }
            }
        }
        
        return r;
    }
}