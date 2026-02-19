using aggregate_api.Application.Domain.Constants;
using aggregate_api.Application.Dtos;
using aggregate_api.Application.Domain.Models;
using aggregate_api.Core.FluentValidators.Services.Contracts;
using aggregate_api.Infrastructure.Contracts;
using AutoMapper;
using aggregate_api.Application.Interfaces;
using aggregate_api.Infrastructure;
using FluentValidation;

namespace aggregate_api.Application.Services;

public class AggregateService : IAggregateService
{
    private readonly ILoggingService _loggingService;
    private readonly IEnvironmentService _environmentService;
    private readonly IFluentValidationService _fluentValidationService;
    private readonly IValidator<CustomerAggregationCommand> _aggregateDtoValidator;
    private readonly IMapper _mapper;
    private readonly List<ITransactionSource> _transactionSources;
    private readonly ITransactionNormaliser _transactionNormaliser;
    private readonly ITransactionCategoriser _transactionCategoriser;

    public AggregateService(
        ILoggingService loggingService,
        IEnvironmentService environmentService,
        IFluentValidationService fluentValidationService,
        IMapper mapper,
        IEnumerable<ITransactionSource> dataSources,
        ITransactionNormaliser normaliser,
        ITransactionCategoriser categoriser,
        IValidator<CustomerAggregationCommand> aggregateValidator)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _environmentService = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
        _fluentValidationService = fluentValidationService ?? throw new ArgumentNullException(nameof(fluentValidationService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _transactionSources = dataSources.ToList();
        _transactionCategoriser = categoriser;
        _transactionNormaliser = normaliser;
        _aggregateDtoValidator = aggregateValidator ?? throw new ArgumentNullException(nameof(aggregateValidator));
    }

    public async Task<ResponseModel<List<AggregatedCustomerTransactionsDto>>> AggregateClientsAsync(
        CustomerAggregationCommand customerAggregationCommand,
        CancellationToken token)
    {
        _loggingService.LogTrace(LoggingMessages.Executing("AggregationService", "AggregateClientsAsync"));

        var response = new ResponseModel<List<AggregatedCustomerTransactionsDto>>(new List<AggregatedCustomerTransactionsDto>());

        response.MergeResponses(_fluentValidationService.ValidateAggregateCommand(
            customerAggregationCommand, _aggregateDtoValidator));

        if (!response.IsValid) return response;

        try
        {
            foreach (var customerId in customerAggregationCommand.CustomerIds)
            {
                token.ThrowIfCancellationRequested();

                var filter = new CustomerTransactionFilter
                {
                    CustomerID = customerId,
                    FromDate = customerAggregationCommand.FromDate,
                    ToDate = customerAggregationCommand.ToDate
                };

                var aggregatedCustomer = await AggregateCustomerAsync(filter, token);

                response.Data.Add(aggregatedCustomer);
            }

            return response;
        }
        catch (OperationCanceledException)
        {
            _loggingService.LogWarning(LoggingMessages.Exception("AggregationService", "Request Cancelled"));
            response.Errors.Add("Request was Cancelled");
            return response;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(LoggingMessages.Exception("AggregationService", "Unexpected Failure"), ex);
            response.Errors.Add("An unexpected error occurred while processing the request");
            return response;
        }
    }

    private async Task<AggregatedCustomerTransactionsDto> AggregateCustomerAsync(
        CustomerTransactionFilter filter,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        
        var allRaw = new List<RawTransaction>();

        foreach (var source in _transactionSources)
        {
            try
            {
                var sourceTransactions = await source.GettransactionsAsync(
                    filter.CustomerID,
                    token);
                
                if (sourceTransactions != null)
                    allRaw.AddRange(sourceTransactions);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(
                    LoggingMessages.Exception(nameof(AggregateService),
                    $"Source {source.GetType().Name} failed for customer {filter.CustomerID}"), ex);
            }
        }

        if (!allRaw.Any())
        {
            _loggingService.LogError(
                LoggingMessages.Exception(
                    nameof(AggregateService),
                    $"Failed to retrieve transactions for customer {filter.CustomerID}"));
        }

        var normalisedTransactions = allRaw
            .Select(t => _transactionNormaliser.Normalise(t, t.Source))
            .ToList();

        var filteredTransactions = normalisedTransactions
            .Where(t =>
                t.CustomerID == filter.CustomerID &&
                (!filter.FromDate.HasValue || t.TransactionDate >= filter.FromDate.Value) &&
                (!filter.ToDate.HasValue || t.TransactionDate <= filter.ToDate.Value))
            .ToList();

        var categorisedTransactions = _transactionCategoriser.Categorise(filteredTransactions);

        var aggregatedTransactions = categorisedTransactions
            .GroupBy(a => a.Category)
            .Select(s => new AggregatedCategoryResultsDtos
            {
                Category = s.Key,
                Amount = s.Sum(t => t.Amount),
                TransactionCount = s.Count()
            })
            .ToList();

        return new AggregatedCustomerTransactionsDto
        {
            CustomerID = filter.CustomerID,
            CategoryAggregates = aggregatedTransactions
        };
    }
}
