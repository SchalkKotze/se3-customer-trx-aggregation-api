using aggregate_api.Application.Domain.Constants;
using aggregate_api.Application.Domain.Models;
using aggregate_api.Application.Dtos;
using aggregate_api.Application.Interfaces;
using aggregate_api.Core.FluentValidators.Services.Contracts;
using aggregate_api.Infrastructure;
using aggregate_api.Infrastructure.Contracts;
using AutoMapper;
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

        _transactionSources = dataSources?.ToList()
            ?? throw new ArgumentNullException(nameof(dataSources));

        _transactionNormaliser = normaliser ?? throw new ArgumentNullException(nameof(normaliser));
        _transactionCategoriser = categoriser ?? throw new ArgumentNullException(nameof(categoriser));
        _aggregateDtoValidator = aggregateValidator ?? throw new ArgumentNullException(nameof(aggregateValidator));
    }
    
    //Services Entry Points
    
    public async Task<ResponseModel<List<AggregatedCustomerTransactionsDto>>> AggregateClientsAsync(
        CustomerAggregationCommand customerAggregationCommand,
        CancellationToken token)
    {
        _loggingService.LogTrace(
            LoggingMessages.Executing(nameof(AggregateService), nameof(AggregateClientsAsync)));

        var response =
            new ResponseModel<List<AggregatedCustomerTransactionsDto>>(
                new List<AggregatedCustomerTransactionsDto>());

        response.MergeResponses(
            _fluentValidationService.ValidateAggregateCommand(
                customerAggregationCommand,
                _aggregateDtoValidator));

        if (!response.IsValid)
            return response;

        try
        {
            foreach (var customerId in customerAggregationCommand.CustomerIds)
            {
                token.ThrowIfCancellationRequested();

                var filter = new CustomerTransactionFilter
                {
                    CustomerID = customerId,
                    FromDate = customerAggregationCommand.FromDate,
                    ToDate = customerAggregationCommand.ToDate,
                    SourceSystem = customerAggregationCommand.SourceSystem
                };

                 var transactions =
                await GetNormaliseFilterAndCategoriseAsync(filter, token);

                 var categoryAggregates = transactions
                     .GroupBy(t => t.Category)
                     .Select(g => new AggregatedCategoryResultsDtos
                     {
                         Category = g.Key,
                         Amount = g.Sum(t => t.Amount),
                         TransactionCount = g.Count()
                     })
                     .ToList();

                 response.Data.Add( new AggregatedCustomerTransactionsDto
                 {
                     CustomerID = filter.CustomerID,
                     CategoryAggregates = categoryAggregates
                 });
            }

            return response;
        }
        catch (OperationCanceledException)
        {
            _loggingService.LogWarning(
                LoggingMessages.Exception(
                    nameof(AggregateService),
                    "Request Cancelled"));

            response.Errors.Add("Request was cancelled");
            return response;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(
                LoggingMessages.Exception(
                    nameof(AggregateService),
                    "Unexpected Failure"),
                ex);

            response.Errors.Add(
                "An unexpected error occurred while processing the request");

            return response;
        }
    }



    public async Task<ResponseModel<List<CustomerBalanceDto>>> GetBalancesAsync(
    CustomerAggregationCommand command,
    CancellationToken token)
{
    _loggingService.LogTrace(
        LoggingMessages.Executing(nameof(AggregateService), nameof(GetBalancesAsync)));

    var response =
        new ResponseModel<List<CustomerBalanceDto>>(new List<CustomerBalanceDto>());

    response.MergeResponses(
        _fluentValidationService.ValidateAggregateCommand(
            command,
            _aggregateDtoValidator));

    if (!response.IsValid)
        return response;

    try
    {
        foreach (var customerId in command.CustomerIds)
        {
            token.ThrowIfCancellationRequested();

            var filter = new CustomerTransactionFilter
            {
                CustomerID = customerId,
                FromDate = command.FromDate,
                ToDate = command.ToDate,
                SourceSystem = command.SourceSystem
            };
            
            var transactions =
                await GetNormaliseFilterAndCategoriseAsync(filter, token);

            var balance = transactions.Sum(t => t.Amount);

            response.Data.Add(new CustomerBalanceDto
            {
                CustomerID = customerId,
                Balance = balance
            });
        }

        return response;
    }
    catch (OperationCanceledException)
    {
        _loggingService.LogWarning(
            LoggingMessages.Exception(
                nameof(AggregateService),
                "Request Cancelled"));

        response.Errors.Add("Request was cancelled");
        return response;
    }
    catch (Exception ex)
    {
        _loggingService.LogError(
            LoggingMessages.Exception(
                nameof(AggregateService),
                "Unexpected Failure"),
            ex);

        response.Errors.Add(
            "An unexpected error occurred while processing balances");

        return response;
    }
}


public async Task<ResponseModel<List<SpendByCategoryDto>>> GetSpendByCategoryAsync(
    CustomerAggregationCommand command,
    CancellationToken token)
{
    _loggingService.LogTrace(
        LoggingMessages.Executing(nameof(AggregateService), nameof(GetSpendByCategoryAsync)));

    var response =
        new ResponseModel<List<SpendByCategoryDto>>(
            new List<SpendByCategoryDto>());

    response.MergeResponses(
        _fluentValidationService.ValidateAggregateCommand(
            command,
            _aggregateDtoValidator));

    if (!response.IsValid)
        return response;

    try
    {
        foreach (var customerId in command.CustomerIds)
        {
            token.ThrowIfCancellationRequested();

            var filter = new CustomerTransactionFilter
            {
                CustomerID = customerId,
                FromDate = command.FromDate,
                ToDate = command.ToDate,
                SourceSystem = command.SourceSystem
            };

         
            var transactions =
                await GetNormaliseFilterAndCategoriseAsync(filter, token);

            var spentByCategory = transactions
                .GroupBy(t => t.Category)
                .Select(g => new SpendByCategoryDto
                {
                    CustomerId = customerId,
                    Category = g.Key,
                    TotalSpent = Math.Abs(g.Sum(t => t.Amount)),
                    Currency = g.First().Currency
                });

            response.Data.AddRange(spentByCategory);
        }

        return response;
    }
    catch (OperationCanceledException)
    {
        _loggingService.LogWarning(
            LoggingMessages.Exception(
                nameof(AggregateService),
                "Request Cancelled"));

        response.Errors.Add("Request was cancelled");
        return response;
    }
    catch (Exception ex)
    {
        _loggingService.LogError(
            LoggingMessages.Exception(
                nameof(AggregateService),
                "Unexpected Failure"),
            ex);

        response.Errors.Add(
            "An unexpected error occurred while processing spent by category");

        return response;
    }
}

public async Task<ResponseModel<List<CustomerMonthlySummaryDto>>> GetMonthlySummaryAsync(
    CustomerAggregationCommand command,
    CancellationToken token)
{
    _loggingService.LogTrace(
        LoggingMessages.Executing(nameof(AggregateService), nameof(GetMonthlySummaryAsync)));

    var response =
        new ResponseModel<List<CustomerMonthlySummaryDto>>(
            new List<CustomerMonthlySummaryDto>());

    response.MergeResponses(
        _fluentValidationService.ValidateAggregateCommand(
            command,
            _aggregateDtoValidator));

    if (!response.IsValid)
        return response;

    try
    {
        foreach (var customerId in command.CustomerIds)
        {
            token.ThrowIfCancellationRequested();

            var filter = new CustomerTransactionFilter
            {
                CustomerID = customerId,
                FromDate = command.FromDate,
                ToDate = command.ToDate,
                SourceSystem = command.SourceSystem
            };

        
            var transactions =
                await GetNormaliseFilterAndCategoriseAsync(filter, token);

            var monthlySummaries = transactions
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .Select(g =>
                {
                    var inflow = g.Where(t => t.Amount > 0).Sum(t => t.Amount);
                    var outflow = g.Where(t => t.Amount < 0).Sum(t => t.Amount);

                    return new CustomerMonthlySummaryDto
                    {
                        CustomerID = customerId,
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalInflow = inflow,
                        TotalOutflow = Math.Abs(outflow),
                        NetAmount = inflow + outflow,
                        TransactionCount = g.Count()
                    };
                })
                .OrderBy(r => r.Year)
                .ThenBy(r => r.Month);

            response.Data.AddRange(monthlySummaries);
        }

        return response;
    }
    catch (OperationCanceledException)
    {
        _loggingService.LogWarning(
            LoggingMessages.Exception(
                nameof(AggregateService),
                "Request Cancelled"));

        response.Errors.Add("Request was cancelled");
        return response;
    }
    catch (Exception ex)
    {
        _loggingService.LogError(
            LoggingMessages.Exception(
                nameof(AggregateService),
                "Unexpected Failure"),
            ex);

        response.Errors.Add(
            "An unexpected error occurred while processing monthly summary");

        return response;
    }
}
    
// Private Methods
    
    // Main Pipe for Data : Gets Data
    //                      Normalises Data 
    //                      Filters data based on object
    //                      Categorises data
    //                      And returns List of Uniform Transactions
    
    private async Task<List<Transaction>> GetNormaliseFilterAndCategoriseAsync(
        CustomerTransactionFilter filter,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var allRawTransactions = new List<RawTransaction>();

     
        foreach (var source in _transactionSources)
        {
            try
            {
                var sourceTransactions =
                    await source.GettransactionsAsync(filter.CustomerID, token);

                if (sourceTransactions != null)
                    allRawTransactions.AddRange(sourceTransactions);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(
                    LoggingMessages.Exception(
                        nameof(AggregateService),
                        $"Source {source.GetType().Name} failed for customer {filter.CustomerID}"),
                    ex);
            }
        }

        if (!allRawTransactions.Any())
        {
            _loggingService.LogWarning(
                LoggingMessages.Exception(
                    nameof(AggregateService),
                    $"No transactions retrieved for customer {filter.CustomerID}"));

            return new List<Transaction>();
        }
        
        var normalisedTransactions = allRawTransactions
            .Select(t => _transactionNormaliser.Normalise(t, t.Source))
            .ToList();
        
        var filteredTransactions = normalisedTransactions
            .Where(t =>
                t.CustomerID == filter.CustomerID &&
                (string.IsNullOrWhiteSpace(filter.SourceSystem) ||
                 t.Source == filter.SourceSystem) &&
                (!filter.FromDate.HasValue ||
                 t.TransactionDate >= filter.FromDate.Value) &&
                (!filter.ToDate.HasValue ||
                 t.TransactionDate <= filter.ToDate.Value))
            .ToList();
        
        var categorisedTransactions =
            _transactionCategoriser.Categorise(filteredTransactions);

        return categorisedTransactions;
    }
}