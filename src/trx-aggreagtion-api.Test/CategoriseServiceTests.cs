using System.Runtime.InteropServices.JavaScript;
using System.Text;

using aggregate_api.Application.Domain.Models;
using aggregate_api.Application.Domain.Requests.v1;
using aggregate_api.Application.Domain.Responses;
using aggregate_api.Application.Dtos;
using aggregate_api.Application.Infrastructure.Categorisation;
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

public class CategoriseServiceTests
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


    public CategoriseServiceTests()
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

        var realCategoriser = new TransactionCategoriser();
        
        _service_under_test = new AggregateService(
            _loggingService.Object,
            _environmentService.Object,
            _fluentValidationService .Object,
            _mapper.Object,
            _transactionSources,
            _normaliser.Object,
            realCategoriser,
            _aggregateValidator .Object
            );
    }
    
    [Fact]
    public async Task AggregateClientAsync_CategorisesTransactionCorrectly()
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
                Source = "BX",
                CustomerID = customerId,
                Amount = 100,
                TransactiopnDate = DateTime.UtcNow.AddDays(-10),
                Description = "TicketPro"
            },
            new BankSourceRawTransaction
            {
                Source = "BX",
                CustomerID = customerId,
                Amount = 1000,
                TransactiopnDate = DateTime.UtcNow.AddDays(-40),
                Description = "FoodLovers"
            }
        };
        
        
        _bankSource.Setup(s => s.GettransactionsAsync(customerId))
            .ReturnsAsync(bankTransactions);

        
        _normaliser
            .Setup(n=>n.Normalise(It.IsAny<object>(),It.IsAny<string>()))
            .Returns((object t ,string s) =>
            {
                string description = string.Empty;
                decimal amt = 0;

                switch (t)
                {
                    case BankSourceRawTransaction b:
                        description = b.Description ?? string.Empty;
                        amt = b.Amount;
                        break;
                    case CreditRawTransactions c:
                        description = c.Description;
                        amt = c.AmountCents;
                        break;
                    
                }
               
                return new Transaction
                {
                    CustomerID = customerId,
                    Amount = amt,
                    TransactionDate = DateTime.UtcNow,
                    Source = s,
                    Description = description
                };
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
        
        Assert.Equal(2, test_result.Data.First().CategoryAggregates.Count);

        
    }
    
}