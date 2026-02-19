using System.Security.Claims;
using aggregate_api.Application.Domain.Constants;
using aggregate_api.Application.Domain.Requests.v1;
using aggregate_api.Application.Interfaces;
using aggregate_api.Core.Swagger.Filters;
using aggregate_api.Infrastructure.Contracts;
using AutoMapper;
using aggregate_api.Core.AutoMapper.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace aggregate_api.Application.Controllers.v1;

[ApiController]
[Route("api/v1/aggregation")]
[Authorize]
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

    // ------------------------------------------------------------------
    // 1. Aggregate SINGLE customer (optional date filtering)
    // GET /api/v1/aggregation/customers/{customerId}?days=30
    // GET /api/v1/aggregation/customers/{customerId}?fromDate=...&toDate=...
    // ------------------------------------------------------------------
    [HttpGet("customers/{customerId}")]
    [SwaggerOperationFilter(typeof(SwaggerResponseFilter))]
    [ApiExplorerSettings(GroupName = "v1")]
    public async Task<IActionResult> AggregateSingleCustomerAsync(
        string customerId,
        int? days,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken token)
    {
        try
        {
            if (days.HasValue && (fromDate.HasValue || toDate.HasValue))
                return BadRequest("Use either 'days' OR 'fromDate/toDate', not both.");

            if (days.HasValue && days <= 0)
                return BadRequest("Days must be greater than zero.");

            var command = new CustomerAggregationCommand
            {
                CorrelationId = Guid.NewGuid().ToString(),
                CustomerIds = new[] { customerId },
                FromDate = days.HasValue ? DateTime.UtcNow.AddDays(-days.Value) : fromDate,
                ToDate = days.HasValue ? DateTime.UtcNow : toDate
            };

            var response = await _aggregateService.AggregateClientsAsync(command, token);

            return response.IsValid
                ? Ok(response.Data.Single())
                : BadRequest(response);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499); // Client Closed Request (useful signal)
        }
        catch (Exception ex)
        {
            _loggingService.LogError(
                LoggingMessages.Exception(nameof(AggregationController), nameof(AggregateSingleCustomerAsync)),
                ex);

            return StatusCode(500);
        }
    }

    // ------------------------------------------------------------------
    // 2. Aggregate MULTIPLE customers
    // POST /api/v1/aggregation/customers
    // ------------------------------------------------------------------
    [HttpPost("customers")]
    [SwaggerOperationFilter(typeof(SwaggerResponseFilter))]
    [ApiExplorerSettings(GroupName = "v1")]
    public async Task<IActionResult> AggregateCustomersAsync(
        SendAggregateRequest request,
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
            return StatusCode(499);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(
                LoggingMessages.Exception(nameof(AggregationController), nameof(AggregateCustomersAsync)),
                ex);

            return StatusCode(500);
        }
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------
    private string GetClaimValue(string claimType)
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        return claimsIdentity?.Claims.FirstOrDefault(c => c.Type == claimType)?.Value ?? string.Empty;
    }

    private string GetRoleValues()
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        var roles = claimsIdentity?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value);

        return roles != null ? string.Join(",", roles) : string.Empty;
    }
}
