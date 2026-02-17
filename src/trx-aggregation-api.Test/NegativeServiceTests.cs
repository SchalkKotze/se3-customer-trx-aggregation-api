using System.Runtime.InteropServices.JavaScript;
using System.Text;

using aggregate_api.Application.Domain.Models;
using aggregate_api.Application.Domain.Requests.v1;
using aggregate_api.Application.Domain.Responses;
using aggregate_api.Application.Dtos;
using aggregate_api.Application.Infrastructure.Categorisation;
using aggregate_api.Application.Interfaces;
using aggregate_api.Application.Services;
using aggregate_api.Core.FluentValidators;
using aggregate_api.Core.FluentValidators.Services;
using aggregate_api.Core.FluentValidators.Services.Contracts;
using aggregate_api.Infrastructure;
using aggregate_api.Infrastructure.Contracts;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using TransactionCategory = aggregate_api.Application.Domain.Enums.TransactionCategory;

namespace trx_aggreagtion_api.Test;

public class NagativeServiceTests
{
    private readonly Mock<ITransactionNormaliser> _normaliser = new() ;
    private readonly Mock<ITransactionCategoriser> _categoriser = new();
    private readonly Mock<ITransactionSource> _bankSource = new();
    private readonly Mock<ITransactionSource> _creditSource = new();
    private readonly Mock<ILoggingService> _loggingService = new();
    private readonly Mock<IEnvironmentService> _environmentService = new();
    private readonly IFluentValidationService _fluentValidationService;
    private readonly Mock<List<ITransactionSource>> _transactionSources = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly IValidator<CustomerAggregationCommand> _aggregateValidator;
    
    private AggregateService _service_under_test = default!;


    public NagativeServiceTests()
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

        var realValidator = new FluentValidationService(_loggingService.Object);
        _aggregateValidator = new AggregateCommandValidator();
        
        _service_under_test = new AggregateService(
            _loggingService.Object,
            _environmentService.Object,
            realValidator,
            _mapper.Object,
            _transactionSources,
            _normaliser.Object,
            _categoriser.Object,
            _aggregateValidator
            );
    }
    
    [Fact]
    public async Task AggregateClientAsync_WhenCustomerListEmpty_ReturnEmptyResult()
    {
        var command = new CustomerAggregationCommand
        {
            CustomerIds = new List<string>()
        };
        
        var test_result = await _service_under_test.AggregateClientsAsync(command,CancellationToken.None);
 
        //Assert 

        Assert.NotNull(test_result);
        Assert.True(test_result.IsValid);
        Assert.Empty(test_result.Data);
        
    }
    
}