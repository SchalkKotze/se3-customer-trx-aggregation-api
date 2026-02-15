using aggregate_api.Application.Domain.Models;
using FluentValidation;

namespace aggregate_api.Core.FluentValidators.Services.Contracts;

public interface IFluentValidationService
{
    ResponseModel ValidateAggregateCommand<TDto>(TDto dto, IValidator<TDto> validator);
    ResponseModel ValidateDtos<TDto>(IList<TDto> dtos, IValidator<TDto> validator);
}