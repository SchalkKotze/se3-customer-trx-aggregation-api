using System.Transactions;
using aggregate_api.Application.Domain.Requests.v1;
using aggregate_api.Application.Dtos;
using aggregate_api.Application.Interfaces;
using aggregate_api.Application.Services;
using aggregate_api.Core.FluentValidators.Services.Contracts;
using aggregate_api.Infrastructure;
using aggregate_api.Infrastructure.Contracts;
using AutoMapper;
using FluentValidation;
using Moq;

namespace trx_aggreagtion_api.Test;

public class AggregateServiceTests
{
    private readonly Mock<ITransactionNormaliser> _normaliser = new() ;
    private readonly Mock<ITransactionCategoriser> _categoriser = new();
    private readonly Mock<ITransactionSource> _bankSource = new();
    private readonly Mock<ITransactionSource> _creditSource = new();
    private readonly Mock<ILoggingService> _loggingService = new();
    private readonly Mock<IEnvironmentService> _environmentService = new();
    private readonly Mock<IFluentValidationService> _fluentValidationService = new();
    private readonly Mock<List<ITransactionSource>> _transactionSources = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IValidator<CustomerAggregationCommand>> _aggregateValidator= new();
    
    private AggregateService _service_under_test = default!;


    public AggregateServiceTests()
    {
        _mapper
            .Setup(m=>m.Map<CustomerAggregationCommand>
                (It.IsAny<SendAggregateRequest>()))
            .Returns(new Func<object, CustomerAggregationCommand>((src) => new CustomerAggregationCommand
            {
                CustomerIds = ((SendAggregateRequest)src).CustomerIds
            }));


        var _transactionSources = new List<ITransactionSource>
        {
            _bankSource.Object,
            _creditSource.Object,
        };
        
        var _service_under_test = new AggregateService(
            _loggingService.Object,
            _environmentService.Object,
            _fluentValidationService .Object,
            _mapper.Object,
            _transactionSources,
            _normaliser.Object,
            _categoriser.Object,
            _aggregateValidator .Object
            );
    }
    
    [Fact]
    public async Task AggregateClientAsync_WhenValidInput_ReturnsAggregatedResult()
    {
        var customerId = "CustID101";
        var command = new CustomerAggregationCommand
        {
            CustomerIds = new List<string> { customerId }
        };
        var bankTransactions = new List<RawTransaction>
        {
            new BankSourceRawTransaction
            {
                Source = null,
                BankTransactionID = null,
                CustomerID = null,
                Amount = 0,
                Descriptiopn = null,
                TransactiopnDate = default
            }
        };
        
        var  creditTransactions = new List<RawTransaction>
        {
            new CreditRawTransactions
            {
                Source = null,
                CreditGuid = default,
                AccountID = null,
                AmountCents = 0,
                Merchant = null,
                TimeStamp = null,
                Description = null
            }
        };

        _bankSource
            .Setup(s => s.GettransactionsAsync(customerId))
            .ReturnsAsync(bankTransactions);
        _creditSource
            .Setup(s => s.GettransactionsAsync(customerId))
            .ReturnsAsync(creditTransactions);

        var _dataSource = new List<ITransactionSource>
        {
            _bankSource.Object,
            _creditSource.Object
        };
        
        _service_under_test = new AggregateService(
            _loggingService.Object,
            _environmentService.Object,
            _fluentValidationService.Object,
            _mapper.Object,
            _dataSource,
            _normaliser.Object,
            _categoriser.Object,
            _aggregateValidator.Object
        );
    }

    
}