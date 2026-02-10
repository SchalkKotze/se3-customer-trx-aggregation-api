using aggregate_api.Application.Domain.Constants;
using aggregate_api.Application.Dtos;
using aggregate_api.Application.Domain.Models;
using aggregate_api.Core.FluentValidators.Services.Contracts;
using aggregate_api.Infrastructure.Contracts;
using AutoMapper;
using aggregate_api.Application.Interfaces;
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

    public AggregateService(ILoggingService loggingService,
                        IEnvironmentService environmentService,
                        IFluentValidationService fluentValidationService,
                        IMapper mapper,
                        IEnumerable<ITransactionSource> dataSources,
                        ITransactionNormaliser normaliser,
                        ITransactionCategoriser categoriser,
                        IValidator<CustomerAggregationCommand> aggregateValidator
                        )
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _environmentService = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
        _fluentValidationService = fluentValidationService ?? throw new ArgumentNullException(nameof(fluentValidationService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _transactionSources = dataSources.ToList();
        _transactionCategoriser = categoriser;
        _transactionNormaliser = normaliser;
        _aggregateDtoValidator = aggregateValidator??throw new ArgumentNullException(nameof(aggregateValidator));
    }
    
    public async Task<ResponseModel<List<AggregatedCustomerTransactionsDto>>> AggregateClientsAsync(CustomerAggregationCommand customerAggregationCommand, CancellationToken token)
    {
        _loggingService.LogTrace(LoggingMessages.Executing("AggregationService", "AggregateClientsAsync"));

        var r = new ResponseModel<List<AggregatedCustomerTransactionsDto>>(
            new List<AggregatedCustomerTransactionsDto>());
        
        r.MergeResponses(_fluentValidationService.ValidateAggreateDto(
            customerAggregationCommand, _aggregateDtoValidator));

        if (!r.IsValid) return r;

        var allCustomerAggregates = new List<AggregatedCustomerTransactionsDto>();
       
        foreach (var customerID in customerAggregationCommand.CustomerIds)
        {
            var rawResults = await Task.WhenAll(
                _transactionSources.Select(s => s.GettransactionsAsync(customerID)));

            var allRaw = rawResults.SelectMany(x => x).ToList();
            
            var normalisedTransactions = allRaw
                .Select(t => _transactionNormaliser.Normalise(t,t.Source))
                .ToList();
            
            var filteredTransactions = normalisedTransactions
                .Where(t => (!customerAggregationCommand.FromDate.HasValue ||
                             t.TransactionDate >= customerAggregationCommand.FromDate.Value) &&
                            (!customerAggregationCommand.ToDate.HasValue ||
                             t.TransactionDate <= customerAggregationCommand.ToDate.Value))
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

            var aggregatedCustomer =  new AggregatedCustomerTransactionsDto
            {
                CustomerID = customerID,
                CategoryAggregates = aggregatedTransactions
            };

            allCustomerAggregates.Add(aggregatedCustomer);
        }
        
        r.Data =  allCustomerAggregates;
        return r;
    }
}