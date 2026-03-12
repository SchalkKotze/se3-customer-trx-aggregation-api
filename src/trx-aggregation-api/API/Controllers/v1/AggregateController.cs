using System.Security.Claims;
using aggregate_api.Application.Domain.Constants;
using aggregate_api.Application.Domain.Requests.v1;
using aggregate_api.Application.Dtos;
using aggregate_api.Application.Interfaces;
using aggregate_api.Application.Services.Contracts;
using aggregate_api.Core.AutoMapper.Extentions;
using aggregate_api.Core.ProblemDetails;
using aggregate_api.Core.Swagger.Filters;
using aggregate_api.Infrastructure.Contracts;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace aggregate_api.Application.Controllers.v1;

/// <summary>
/// Example controller showing how to implement HTTP Problem Details responses.
/// Problem Details is defined in RFC 7807 and provides a standardized way to return error information.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/aggregation")]
[Produces("application/json", "application/problem+json")]
public class AggregationControllerWithProblemDetails : ControllerBase
{
    private readonly ILoggingService _loggingService;
    private readonly IMapper _mapper;
    private readonly IAggregateService _aggregateService;

    public AggregationControllerWithProblemDetails(
        ILoggingService loggingService,
        IMapper mapper,
        IAggregateService aggregateService)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _aggregateService = aggregateService ?? throw new ArgumentNullException(nameof(aggregateService));
    }

    /// <summary>
    /// Example: POST endpoint returning Problem Details on error
    /// </summary>
    /// <response code="200">Successfully aggregated customer transactions.</response>
    /// <response code="400">Bad request - Invalid input provided.</response>
    /// <response code="401">Unauthorized - Authentication required.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("categories")]
    [SwaggerOperationFilter(typeof(SwaggerResponseFilter))]
    [ApiExplorerSettings(GroupName = "v1")]
    [ProduceResponseType(StatusCodes.Status200OK)]
    [ProduceResponseType(StatusCodes.Status400BadRequest)]
    [ProduceResponseType(StatusCodes.Status401Unauthorized)]
    [ProduceResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AggregateAsync(
        [FromBody] SendAggregateRequest request,
        CancellationToken token)
    {
        try
        {
            // Validate request
            if (request == null || request.CustomerIds == null || !request.CustomerIds.Any())
            {
                var problem = ProblemDetailsFactory.CreateBadRequestProblem(
                    detail: "CustomerIds cannot be empty.",
                    instance: HttpContext.Request.Path
                );
                return BadRequest(problem);
            }

            var command = _mapper.Map<CustomerAggregationCommand>(request)
                .WithAppId(GetClaimValue("appid"))
                .WithAppDisplayName(GetClaimValue("app_displayname"))
                .WithUserRoles(GetRoleValues())
                .WithCorrelationId(Guid.NewGuid().ToString());

            var response = await _aggregateService.AggregateClientsAsync(command, token);

            if (!response.IsValid)
            {
                var errors = new Dictionary<string, string[]>
                {
                    { "errors", response.Errors.ToArray() }
                };
                var problem = ProblemDetailsFactory.CreateValidationErrorsProblem(
                    errors: errors,
                    instance: HttpContext.Request.Path
                );
                return UnprocessableEntity(problem);
            }

            return Ok(response.Data);
        }
        catch (OperationCanceledException)
        {
            var problem = ProblemDetailsFactory.CreateProblem(
                status: StatusCodes.Status499ClientClosedRequest,
                title: "Client Closed Request",
                detail: "The request was cancelled by the client.",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                instance: HttpContext.Request.Path
            );
            return StatusCode(StatusCodes.Status499ClientClosedRequest, problem);
        }
        catch (Exception ex)
        {
            var traceId = HttpContext.TraceIdentifier;
            _loggingService.LogError(
                LoggingMessages.Exception(nameof(AggregationControllerWithProblemDetails), nameof(AggregateAsync)), ex);

            var problem = ProblemDetailsFactory.CreateInternalServerErrorProblem(
                detail: "An unexpected error occurred while processing your request.",
                instance: HttpContext.Request.Path,
                traceId: traceId
            );
            return StatusCode(StatusCodes.Status500InternalServerError, problem);
        }
    }

    /// <summary>
    /// Example: GET endpoint returning Problem Details on error
    /// </summary>
    /// <response code="200">Successfully retrieved customer balances.</response>
    /// <response code="400">Bad request - Invalid customer IDs.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("balances")]
    [SwaggerOperationFilter(typeof(SwaggerResponseFilter))]
    [ApiExplorerSettings(GroupName = "v1")]
    [ProduceResponseType(StatusCodes.Status200OK)]
    [ProduceResponseType(StatusCodes.Status400BadRequest)]
    [ProduceResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBalancesAsync(
        [FromQuery] List<string> customerIds,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken token)
    {
        try
        {
            // Validate customer IDs
            if (customerIds == null || !customerIds.Any())
            {
                var problem = ProblemDetailsFactory.CreateBadRequestProblem(
                    detail: "At least one customer ID must be provided.",
                    instance: HttpContext.Request.Path
                );
                return BadRequest(problem);
            }

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

            if (!response.IsValid)
            {
                var errors = new Dictionary<string, string[]>
                {
                    { "errors", response.Errors.ToArray() }
                };
                var problem = ProblemDetailsFactory.CreateValidationErrorsProblem(
                    errors: errors,
                    instance: HttpContext.Request.Path
                );
                return UnprocessableEntity(problem);
            }

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            var problem = ProblemDetailsFactory.CreateProblem(
                status: StatusCodes.Status499ClientClosedRequest,
                title: "Client Closed Request",
                detail: "The request was cancelled by the client.",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                instance: HttpContext.Request.Path
            );
            return StatusCode(StatusCodes.Status499ClientClosedRequest, problem);
        }
        catch (Exception ex)
        {
            var traceId = HttpContext.TraceIdentifier;
            _loggingService.LogError(
                LoggingMessages.Exception(nameof(AggregationControllerWithProblemDetails), nameof(GetBalancesAsync)), ex);

            var problem = ProblemDetailsFactory.CreateInternalServerErrorProblem(
                detail: "An unexpected error occurred while retrieving balances.",
                instance: HttpContext.Request.Path,
                traceId: traceId
            );
            return StatusCode(StatusCodes.Status500InternalServerError, problem);
        }
    }

    /// <summary>
    /// Example: GET endpoint with custom validation
    /// </summary>
    [HttpGet("spent-by-category")]
    [ProduceResponseType(StatusCodes.Status200OK)]
    [ProduceResponseType(StatusCodes.Status400BadRequest)]
    [ProduceResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSpentByCategory(
        [FromQuery] List<string> customerIds,
        CancellationToken cancellationToken)
    {
        try
        {
            if (customerIds == null || !customerIds.Any())
            {
                var problem = ProblemDetailsFactory.CreateBadRequestProblem(
                    detail: "At least one customer ID must be provided.",
                    instance: HttpContext.Request.Path
                );
                return BadRequest(problem);
            }

            var command = new CustomerAggregationCommand
            {
                CorrelationId = Guid.NewGuid().ToString(),
                EventTriggerDate = DateTime.UtcNow,
                CustomerIds = customerIds
            }
            .WithAppId(GetClaimValue("appid"))
            .WithAppDisplayName(GetClaimValue("app_displayname"))
            .WithUserRoles(GetRoleValues());

            var response = await _aggregateService.GetSpendByCategoryAsync(command, cancellationToken);

            if (!response.IsValid)
            {
                var errors = new Dictionary<string, string[]>
                {
                    { "errors", response.Errors.ToArray() }
                };
                var problem = ProblemDetailsFactory.CreateValidationErrorsProblem(
                    errors: errors,
                    instance: HttpContext.Request.Path
                );
                return UnprocessableEntity(problem);
            }

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            var problem = ProblemDetailsFactory.CreateProblem(
                status: StatusCodes.Status499ClientClosedRequest,
                title: "Client Closed Request",
                detail: "The request was cancelled by the client.",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                instance: HttpContext.Request.Path
            );
            return StatusCode(StatusCodes.Status499ClientClosedRequest, problem);
        }
        catch (Exception ex)
        {
            var traceId = HttpContext.TraceIdentifier;
            _loggingService.LogError(
                LoggingMessages.Exception(nameof(AggregationControllerWithProblemDetails), nameof(GetSpentByCategory)), ex);

            var problem = ProblemDetailsFactory.CreateInternalServerErrorProblem(
                detail: "An unexpected error occurred while retrieving spend by category.",
                instance: HttpContext.Request.Path,
                traceId: traceId
            );
            return StatusCode(StatusCodes.Status500InternalServerError, problem);
        }
    }

    /// <summary>
    /// Example: GET endpoint with date range validation
    /// </summary>
    [HttpGet("monthly-summary")]
    [ApiExplorerSettings(GroupName = "v1")]
    [ProduceResponseType(StatusCodes.Status200OK)]
    [ProduceResponseType(StatusCodes.Status400BadRequest)]
    [ProduceResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProduceResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMonthlySummaryAsync(
        [FromQuery] List<string> customerIds,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken token)
    {
        try
        {
            // Validate customer IDs
            if (customerIds == null || !customerIds.Any())
            {
                var problem = ProblemDetailsFactory.CreateBadRequestProblem(
                    detail: "At least one customer ID must be provided.",
                    instance: HttpContext.Request.Path
                );
                return BadRequest(problem);
            }

            // Validate date range
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                var errors = new Dictionary<string, string[]>
                {
                    { "dateRange", new[] { "fromDate must be before or equal to toDate." } }
                };
                var problem = ProblemDetailsFactory.CreateValidationErrorsProblem(
                    errors: errors,
                    instance: HttpContext.Request.Path
                );
                return UnprocessableEntity(problem);
            }

            var command = new CustomerAggregationCommand
            {
                CorrelationId = Guid.NewGuid().ToString(),
                CustomerIds = customerIds,
                FromDate = fromDate,
                ToDate = toDate,
                EventTriggerDate = DateTime.UtcNow
            }
            .WithAppId(GetClaimValue("appid"))
            .WithAppDisplayName(GetClaimValue("app_displayname"))
            .WithUserRoles(GetRoleValues());

            var response = await _aggregateService.GetMonthlySummaryAsync(command, token);

            if (!response.IsValid)
            {
                var errors = new Dictionary<string, string[]>
                {
                    { "errors", response.Errors.ToArray() }
                };
                var problem = ProblemDetailsFactory.CreateValidationErrorsProblem(
                    errors: errors,
                    instance: HttpContext.Request.Path
                );
                return UnprocessableEntity(problem);
            }

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            var problem = ProblemDetailsFactory.CreateProblem(
                status: StatusCodes.Status499ClientClosedRequest,
                title: "Client Closed Request",
                detail: "The request was cancelled by the client.",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                instance: HttpContext.Request.Path
            );
            return StatusCode(StatusCodes.Status499ClientClosedRequest, problem);
        }
        catch (Exception ex)
        {
            var traceId = HttpContext.TraceIdentifier;
            _loggingService.LogError(
                LoggingMessages.Exception(nameof(AggregationControllerWithProblemDetails), nameof(GetMonthlySummaryAsync)), ex);

            var problem = ProblemDetailsFactory.CreateInternalServerErrorProblem(
                detail: "An unexpected error occurred while retrieving monthly summary.",
                instance: HttpContext.Request.Path,
                traceId: traceId
            );
            return StatusCode(StatusCodes.Status500InternalServerError, problem);
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
