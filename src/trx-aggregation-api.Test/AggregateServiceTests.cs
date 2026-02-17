using System.Runtime.InteropServices.JavaScript;
using System.Text;

using aggregate_api.Application.Domain.Models;
using aggregate_api.Application.Domain.Requests.v1;
using aggregate_api.Application.Domain.Responses;
using aggregate_api.Application.Dtos;
using aggregate_api.Application.Interfaces;
using aggregate_api.Application.Services;
using aggregate_api.Core.FluentValidators.Services.Contracts;
using aggregate_api.Infrastructure;
using aggregate_api.Infrastructure.Contracts;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using TransactionCategory = aggregate_api.Application.Domain.Enums.TransactionCategory;

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
            .Setup(m => m.Map<CustomerAggregationCommand>
                (It.IsAny<object>()))
            .Returns((object src) =>
            {
                if (src is SendAggregateRequest req)
                {
                    return new CustomerAggregationCommand()
                    {
                        CustomerIds = req.CustomerIds
                    };
                }

                return new CustomerAggregationCommand();
            });


        var _transactionSources = new List<ITransactionSource>
        {
            _bankSource.Object,
            _creditSource.Object,
        };
        
        _service_under_test = new AggregateService(
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
                Source = "B",
                CustomerID = customerId,
                Amount = 10,
                TransactiopnDate = default
            }
        };
        
        var  creditTransactions = new List<RawTransaction>
        {
            new CreditRawTransactions
            {
                Source = "C",
                AccountID = customerId,
                AmountCents = 100,
                TimeStamp = System.DateTime.UtcNow.ToString("o")
            
            }
        };

        _bankSource.Setup(s => s.GettransactionsAsync(customerId))
            .ReturnsAsync(bankTransactions);
        _creditSource
            .Setup(s => s.GettransactionsAsync(customerId))
            .ReturnsAsync(creditTransactions);

        

        _normaliser
            .Setup(n=>n.Normalise(It.IsAny<object>(),It.IsAny<string>()))
            .Returns((object t ,string s) => new Transaction
            {
             CustomerID = customerId,
             Amount = 100,
             
            });

        _categoriser
            .Setup(c => c.Categorise(It.IsAny<IEnumerable<Transaction>>()))
            .Returns((IEnumerable<Transaction> transactions) => transactions.ToList());


        _fluentValidationService
            .Setup(f => f.ValidateAggregateCommand(
                It.IsAny<CustomerAggregationCommand>(),
                It.IsAny<IValidator<CustomerAggregationCommand>>()))
            .Returns(new ResponseModel<List<AggregatedCustomerTransactionsDto>>(
                new List<AggregatedCustomerTransactionsDto>()));
                    
        
        _aggregateValidator
            .Setup(a => a.Validate(It.IsAny<CustomerAggregationCommand>()))
            .Returns(new ValidationResult());


        var test_result = await _service_under_test.AggregateClientsAsync(command,CancellationToken.None);
 
        //Assert 

        Assert.NotNull(test_result);
        Assert.True(test_result.IsValid);
        Assert.Single(test_result.Data);
        Assert.Equal(customerId, test_result.Data.First().CustomerID);
        
        _bankSource.Verify(s=>s.GettransactionsAsync(customerId),Times.Once);
        _creditSource.Verify(s=>s.GettransactionsAsync(customerId),Times.Once);
        
        _normaliser.Verify(n=>n.Normalise(It.IsAny<object>(),It.IsAny<string>()),Times.Exactly(2));
        _categoriser.Verify(c=>c.Categorise(It.IsAny<IEnumerable<Transaction>>()),Times.Once);
    }

    [Fact]
    public async Task AggregateClientAsync_WhendateRangeSet_FilterTransactionCorrectly()
    {
        var customerId = "CustID101";
        
        var fromDate = DateTime.UtcNow.AddDays(-30);
        var toDate = DateTime.UtcNow;
        
        var command = new CustomerAggregationCommand
        {
            CustomerIds = new List<string> { customerId },
            FromDate = fromDate,
            ToDate = toDate
        };
        var bankTransactions = new List<RawTransaction>
        {
            new BankSourceRawTransaction
            {
                Source = "B",
                CustomerID = customerId,
                Amount = 100,
                TransactiopnDate = DateTime.UtcNow.AddDays(-10)
            },
            new BankSourceRawTransaction
            {
                Source = "B",
                CustomerID = customerId,
                Amount = 1000,
                TransactiopnDate = DateTime.UtcNow.AddDays(-40)
            }
        };
        
        
        _bankSource.Setup(s => s.GettransactionsAsync(customerId))
            .ReturnsAsync(bankTransactions);

        var creditTransactions = new List<RawTransaction>();
        _creditSource
            .Setup(s => s.GettransactionsAsync(customerId))
            .ReturnsAsync(creditTransactions);
        
        _normaliser
            .Setup(n=>n.Normalise(It.IsAny<object>(),It.IsAny<string>()))
            .Returns((object t ,string s) => new Transaction
            {
             CustomerID = customerId,
             Amount = (t as BankSourceRawTransaction)?.Amount??0,
             TransactionDate = (t as BankSourceRawTransaction)?.TransactiopnDate??DateTime.UtcNow,
            });
        
        _categoriser
            .Setup(c => c.Categorise(It.IsAny<IEnumerable<Transaction>>()))
            .Returns((IEnumerable<Transaction> transactions) => transactions.ToList());


        _fluentValidationService
            .Setup(f => f.ValidateAggregateCommand(
                It.IsAny<CustomerAggregationCommand>(),
                It.IsAny<IValidator<CustomerAggregationCommand>>()))
            .Returns(new ResponseModel<List<AggregatedCustomerTransactionsDto>>(
                new List<AggregatedCustomerTransactionsDto>()));
                    
        
        _aggregateValidator
            .Setup(a => a.Validate(It.IsAny<CustomerAggregationCommand>()))
            .Returns(new ValidationResult());


        var test_result = await _service_under_test.AggregateClientsAsync(command,CancellationToken.None);
 
        //Assert 

        Assert.NotNull(test_result);
        Assert.True(test_result.IsValid);
        Assert.Single(test_result.Data);
        
        Assert.Equal(customerId, test_result.Data.First().CustomerID);
        Assert.Equal(100,test_result.Data.First().TotalBalance);
        
    }
}