using System.Security.Claims;
using aggregate_api.Application.Domain.Constants;
using aggregate_api.Application.Domain.Requests.v1;
using aggregate_api.Application.Dtos;
using aggregate_api.Application.Interfaces;
using aggregate_api.Application.Services.Contracts;
using aggregate_api.Core.AutoMapper.Extentions;
using aggregate_api.Core.Swagger.Filters;
using aggregate_api.Infrastructure.Contracts;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace aggregate_api.Application.Controllers.v1;

//GET /aggregates/transactions
//GET /aggregates/balances
//GET /aggregates/spend-by-category
//GET /aggregates/monthly-summary



[ApiController]
[Authorize]
[Route("api/v1/aggregation")]
public class AggregationController : ControllerBase
{
    private readonly ILoggingService _loggingService;
    private readonly IMapper _mapper;
    private readonly IAggregateService _aggregateService;

    public AggregationController(
        ILoggingService loggingService,
        IMapper mapper,
        IAggregateService aggregateService)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _aggregateService = aggregateService ?? throw new ArgumentNullException(nameof(aggregateService));
    }


    [HttpPost]
    [SwaggerOperationFilter(typeof(SwaggerResponseFilter))]
    [ApiExplorerSettings(GroupName = "v1")]
    public async Task<IActionResult> AggregateAsync(
        [FromBody] SendAggregateRequest request,
        CancellationToken token)
    {
        try
        {
            var command = _mapper.Map<CustomerAggregationCommand>(request)
                .WithAppId(GetClaimValue("appid"))
                .WithAppDisplayName(GetClaimValue("app_displayname"))
                .WithUserRoles(GetRoleValues())
                .WithCorrelationId(Guid.NewGuid().ToString());

            var response = await _aggregateService.AggregateClientsAsync(command, token);

            return response.IsValid
                ? Ok(response.Data)
                : BadRequest(response);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(
                LoggingMessages.Exception(nameof(AggregationController), nameof(AggregateAsync)), ex);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
    
[HttpGet("balances")]
[SwaggerOperationFilter(typeof(SwaggerResponseFilter))]
[ApiExplorerSettings(GroupName = "v1")]
public async Task<IActionResult> GetBalancesAsync(
    [FromQuery] List<string> customerIds,
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate,
    CancellationToken token)
{
    try
    {
        var command = new CustomerAggregationCommand
        {
            CorrelationId = Guid.NewGuid().ToString(),
            FromDate = fromDate,
            ToDate = toDate,
            EventTriggerDate = DateTime.UtcNow,
            CustomerIds = customerIds
        }
        .WithAppId(GetClaimValue("appid"))
        .WithAppDisplayName(GetClaimValue("app_displayname"))
        .WithUserRoles(GetRoleValues());

        var response = await _aggregateService.GetBalancesAsync(command, token);

        return response.IsValid
            ? Ok(response)
            : BadRequest(response);
    }
    catch (OperationCanceledException)
    {
        return StatusCode(StatusCodes.Status499ClientClosedRequest);
    }
    catch (Exception ex)
    {
        _loggingService.LogError(
                LoggingMessages.Exception(nameof(AggregationController), nameof(AggregateAsync)), ex);

            return StatusCode(StatusCodes.Status500InternalServerError);
    }
}
    
 
    [HttpGet("spent-by-category")]
    public async Task<IActionResult> GetSpentByCategory(
        [FromQuery] List<string> customerIds,
        CancellationToken cancellationToken)
    {
       
        var command = new CustomerAggregationCommand
        {
            CorrelationId = Guid.NewGuid().ToString(),
            EventTriggerDate = DateTime.UtcNow,
            CustomerIds = customerIds
        }
        .WithAppId(GetClaimValue("appid"))
        .WithAppDisplayName(GetClaimValue("app_displayname"))
        .WithUserRoles(GetRoleValues());

        var result = await _aggregateService.GetSpendByCategoryAsync(command, cancellationToken);

        if (!result.IsValid)
            return StatusCode(207, result); // Multi-Status for partial failures

        return Ok(result);
    }



    [HttpGet("customer/{customerId}")]
    [SwaggerOperationFilter(typeof(SwaggerResponseFilter))]
    [ApiExplorerSettings(GroupName = "v1")]
    public async Task<IActionResult> AggregateSingleCustomerAsync(
        string customerId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken token)
    {
        try
        {
            var command = new CustomerAggregationCommand
            {
                CorrelationId = Guid.NewGuid().ToString(),
                CustomerIds = new[] { customerId },
                FromDate = fromDate,
                ToDate = toDate,
                EventTriggerDate = DateTime.UtcNow
            }
            .WithAppId(GetClaimValue("appid"))
            .WithAppDisplayName(GetClaimValue("app_displayname"))
            .WithUserRoles(GetRoleValues());

            var response = await _aggregateService.AggregateClientsAsync(command, token);

            return response.IsValid
                ? Ok(response.Data.Single())
                : BadRequest(response);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(
                LoggingMessages.Exception(nameof(AggregationController), nameof(AggregateSingleCustomerAsync)), ex);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private string GetClaimValue(string claimType)
    {
        return User.FindFirstValue(claimType) ?? string.Empty;
    }

    private string GetRoleValues()
    {
        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value);

        return string.Join(", ", roles);
    }
}
