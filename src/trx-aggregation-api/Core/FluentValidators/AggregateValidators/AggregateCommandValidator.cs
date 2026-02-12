using aggregate_api.Application.Domain.Constants;
using aggregate_api.Application.Dtos;
using Amazon.CloudWatchLogs.Model;
using FluentValidation;

namespace aggregate_api.Core.FluentValidators;

public class AggregateCommandValidator : AbstractValidator<CustomerAggregationCommand>
{
    public AggregateCommandValidator()
    {
        RuleFor(entity => entity.CustomerIds)
            .NotNull()
            .WithMessage(string.Format(ValidationMessages.NullCheck, "CustomerId"))
            .NotEmpty()
            .WithMessage(string.Format(ValidationMessages.EmptyCheck, "CustomerId"));
        // .GreaterThan(99999999)
        // .WithMessage(string.Format(ValidationMessages.MaximumLengthCheck, "CustomerId", "9"));

    }
}