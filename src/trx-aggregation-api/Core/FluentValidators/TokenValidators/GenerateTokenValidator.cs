using aggregate_api.Application.Domain.Constants;
using aggregate_api.Application.Dtos;
using FluentValidation;

namespace aggregate_api.Core.FluentValidators.TokenValidators;

public class GenerateTokenValidator : AbstractValidator<GenerateTokenDto>
{
    public GenerateTokenValidator()
    {
        RuleFor(entity => entity.ClientId)
            .NotNull()
            .WithMessage(string.Format(ValidationMessages.NullCheck, "ClientId"))
            .NotEmpty()
            .WithMessage(string.Format(ValidationMessages.EmptyCheck, "ClientId"));
        
        RuleFor(entity => entity.ClientSecret)
            .NotNull()
            .WithMessage(string.Format(ValidationMessages.NullCheck, "ClientSecret"))
            .NotEmpty()
            .WithMessage(string.Format(ValidationMessages.EmptyCheck, "ClientSecret"));
    }
}