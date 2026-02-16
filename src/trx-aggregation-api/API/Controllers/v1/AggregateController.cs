using System.Security.Claims;
using aggregate_api.Application.Domain.Constants;
using aggregate_api.Application.Dtos;
using aggregate_api.Application.Domain.Requests.v1;
using aggregate_api.Application.Interfaces;
using aggregate_api.Application.Services.Contracts;
using aggregate_api.Core.Swagger.Filters;
using aggregate_api.Infrastructure.Contracts;
using AutoMapper;
using aggregate_api.Core.AutoMapper.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Swashbuckle.AspNetCore.Annotations;

namespace aggregate_api.Application.Controllers.v1;

[ApiController]
[Route("api/v1/aggregation")]
public class AggregationController : ControllerBase
{
    private readonly ILoggingService _loggingService;
    private readonly IMapper _mapper;
    private readonly IAggregateService _aggregateService;

    public AggregationController(ILoggingService loggingService,
                            IMapper mapper,
                            IAggregateService aggregateService)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _aggregateService = aggregateService ?? throw new ArgumentNullException(nameof(aggregateService));
    }
    
    [HttpPost("unauth-customers")]
    [SwaggerOperationFilter(typeof(SwaggerResponseFilter))]
    [ApiExplorerSettings(GroupName = "v1")]
    public async Task<IActionResult> UnauthAggregateListAsync(SendAggregateRequest request, CancellationToken token)
    {
        try
        {

            var aggregateCommand = _mapper.Map<CustomerAggregationCommand>(request);
            
            var responseModel = await _aggregateService.AggregateClientsAsync(aggregateCommand, token);
            
            if (responseModel.IsValid)
            {
                return Ok(responseModel.Data);
            }

            return BadRequest(responseModel);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(LoggingMessages.Exception("AggregateController", "UnauthAggregateAsync"), ex);
            
            return StatusCode(500);
        }
    }
    
    [HttpGet("unauth-customer")]
    [SwaggerOperationFilter(typeof(SwaggerResponseFilter))]
    [ApiExplorerSettings(GroupName = "v1")]
    public async Task<IActionResult> UnauthAggregateSingleAsync(string request, CancellationToken token)
    {
        try
        {
            var aggregateCommand = new CustomerAggregationCommand
            {
                CorrelationId = Guid.NewGuid().ToString(),
                CustomerIds = new [] {request},
                EventTriggerDate = DateTime.UtcNow
            };
            
            var responseModel = await _aggregateService.AggregateClientsAsync(aggregateCommand, token);
            
            if (responseModel.IsValid)
            {
                return Ok(responseModel.Data.Single());
            }

            return BadRequest(responseModel);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(LoggingMessages.Exception("AggregateController", "UnauthAggregateAsync"), ex);
            
            return StatusCode(500);
        }
    }
    
    [HttpGet("unauth-customer-30day")]
    [SwaggerOperationFilter(typeof(SwaggerResponseFilter))]
    [ApiExplorerSettings(GroupName = "v1")]
    public async Task<IActionResult> UnauthAggregateSingle30Async(string request,
         int days = 30,
        CancellationToken token = default)
    {
        try
        {
            if (days <= 0) return BadRequest("Days must be greater than 0");
            
            var aggregateCommand = new CustomerAggregationCommand
            {
                CorrelationId = Guid.NewGuid().ToString(),
                CustomerIds = new [] {request},
                EventTriggerDate = DateTime.UtcNow,
                FromDate = DateTime.UtcNow.Add(TimeSpan.FromDays(-days)),
                ToDate = DateTime.UtcNow
                
            };
            
            var responseModel = await _aggregateService.AggregateClientsAsync(aggregateCommand, token);
            
            if (responseModel.IsValid)
            {
                return Ok(responseModel.Data.Single());
            }

            return BadRequest(responseModel);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(LoggingMessages.Exception("AggregateController", "UnauthAggregateAsync"), ex);
            
            return StatusCode(500);
        }
    }
    
    [Authorize(Policy = "HtmlPolicy")]
    [HttpPost("customers")]
    [SwaggerOperationFilter(typeof(SwaggerResponseFilter))]
    [ApiExplorerSettings(GroupName = "v1")]
    public async Task<IActionResult> AggregateAsync(SendAggregateRequest request, CancellationToken token)
    {
        try
        {
            var aggregationCommand = _mapper.Map<CustomerAggregationCommand>(request)
                .WithAppId(GetClaimValue("appid"))
                .WithAppDisplayName(GetClaimValue("app_displayname"))
                .WithUserRoles(GetRoleValues())
                .WithCorrelationId(string.Empty);

            var responseModel = await _aggregateService.AggregateClientsAsync(aggregationCommand, token);
            
            if (responseModel.IsValid)
            {
                return Ok(responseModel.Data);
            }

            return BadRequest(responseModel);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(LoggingMessages.Exception("AggregateController", "SendAsync"), ex);
            
            return StatusCode(500);
        }
    }
   

    #region Private Functions
    
    private string GetClaimValue(string claimType)
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        var claim = claimsIdentity?.Claims.FirstOrDefault(c => c.Type == claimType);
        return claim?.Value!;
    }
    private string GetRoleValues()
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        var roles = claimsIdentity?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
        return roles != null ? string.Join(", ", roles) : string.Empty;
    }

    #endregion
}