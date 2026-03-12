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

//
//Http Problem Details
//
// Fixtures : Unit Testing
//
// Extending rediness probes
//

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


    [HttpPost("categories")]
    [SwaggerOperationFilter(typeof(SwaggerResponseFilter))]
    [ApiExplorerSettings(GroupName = "v1")]
    [Produces("application/json", "application/problem+json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status500InternalServerError)]
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

            if (!response.IsValid)
            {
                return UnprocessableEntity(ProblemDetailsFactory.CreateValidationErrorsProblem(
                    errors: new Dictionary<string, string[]>
                    {
                        { "errors", response.Errors.ToArray() }
                    },
                    instance: HttpContext.Request.Path
                ));
            }

            if (response.Data == null || !response.Data.Any())
                return NoContent();

            return Ok(response.Data);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(StatusCodes.Status499ClientClosedRequest,
                ProblemDetailsFactory.CreateProblem(
                    status: StatusCodes.Status499ClientClosedRequest,
                    title: "Client Closed Request",
                    detail: "The request was cancelled by the client.",
                    type: "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                    instance: HttpContext.Request.Path
                ));
        }
        catch (Exception ex)
        {
            _loggingService.LogError(
                LoggingMessages.Exception(nameof(AggregationController), nameof(AggregateAsync)), ex);

            return StatusCode(StatusCodes.Status500InternalServerError,
                ProblemDetailsFactory.CreateInternalServerErrorProblem(
                    detail: "An unexpected error occurred while aggregating transactions.",
                    instance: HttpContext.Request.Path,
                    traceId: HttpContext.TraceIdentifier
                ));
        }
    }

[HttpGet("balances")]
[SwaggerOperationFilter(typeof(SwaggerResponseFilter))]
[ApiExplorerSettings(GroupName = "v1")]
[Produces("application/json", "application/problem+json")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
[ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status500InternalServerError)]
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

        if (!response.IsValid)
        {
            return UnprocessableEntity(ProblemDetailsFactory.CreateValidationErrorsProblem(
                errors: new Dictionary<string, string[]>
                {
                    { "errors", response.Errors.ToArray() }
                },
                instance: HttpContext.Request.Path
            ));
        }

        if (response.Data == null || !response.Data.Any())
            return NoContent();

        return Ok(response);
    }
    catch (OperationCanceledException)
    {
        return StatusCode(StatusCodes.Status499ClientClosedRequest,
            ProblemDetailsFactory.CreateProblem(
                status: StatusCodes.Status499ClientClosedRequest,
                title: "Client Closed Request",
                detail: "The request was cancelled by the client.",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                instance: HttpContext.Request.Path
            ));
    }
    catch (Exception ex)
    {
        _loggingService.LogError(
            LoggingMessages.Exception(nameof(AggregationController), nameof(GetBalancesAsync)), ex);

        return StatusCode(StatusCodes.Status500InternalServerError,
            ProblemDetailsFactory.CreateInternalServerErrorProblem(
                detail: "An unexpected error occurred while retrieving balances.",
                instance: HttpContext.Request.Path,
                traceId: HttpContext.TraceIdentifier
            ));
    }
}


    [HttpGet("spent-by-category")]
    [Produces("application/json", "application/problem+json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSpentByCategory(
        [FromQuery] List<string> customerIds,
        CancellationToken cancellationToken)
    {
        try
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

            var response = await _aggregateService.GetSpendByCategoryAsync(command, cancellationToken);

            if (!response.IsValid)
            {
                return UnprocessableEntity(ProblemDetailsFactory.CreateValidationErrorsProblem(
                    errors: new Dictionary<string, string[]>
                    {
                        { "errors", response.Errors.ToArray() }
                    },
                    instance: HttpContext.Request.Path
                ));
            }

            if (response.Data == null || !response.Data.Any())
                return NoContent();

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(StatusCodes.Status499ClientClosedRequest,
                ProblemDetailsFactory.CreateProblem(
                    status: StatusCodes.Status499ClientClosedRequest,
                    title: "Client Closed Request",
                    detail: "The request was cancelled by the client.",
                    type: "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                    instance: HttpContext.Request.Path
                ));
        }
        catch (Exception ex)
        {
            _loggingService.LogError(
                LoggingMessages.Exception(nameof(AggregationController), nameof(GetSpentByCategory)), ex);

            return StatusCode(StatusCodes.Status500InternalServerError,
                ProblemDetailsFactory.CreateInternalServerErrorProblem(
                    detail: "An unexpected error occurred while retrieving spend by category.",
                    instance: HttpContext.Request.Path,
                    traceId: HttpContext.TraceIdentifier
                ));
        }
    }

[HttpGet("monthly-summary")]
[ApiExplorerSettings(GroupName = "v1")]
[Produces("application/json", "application/problem+json")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
[ProducesResponseType(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> GetMonthlySummaryAsync(
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
            return UnprocessableEntity(ProblemDetailsFactory.CreateValidationErrorsProblem(
                errors: new Dictionary<string, string[]>
                {
                    { "errors", response.Errors.ToArray() }
                },
                instance: HttpContext.Request.Path
            ));
        }

        if (response.Data == null || !response.Data.Any())
            return NoContent();

        return Ok(response);
    }
    catch (OperationCanceledException)
    {
        return StatusCode(StatusCodes.Status499ClientClosedRequest,
            ProblemDetailsFactory.CreateProblem(
                status: StatusCodes.Status499ClientClosedRequest,
                title: "Client Closed Request",
                detail: "The request was cancelled by the client.",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                instance: HttpContext.Request.Path
            ));
    }
    catch (Exception ex)
    {
        _loggingService.LogError(
            LoggingMessages.Exception(nameof(AggregationController), nameof(GetMonthlySummaryAsync)), ex);

        return StatusCode(StatusCodes.Status500InternalServerError,
            ProblemDetailsFactory.CreateInternalServerErrorProblem(
                detail: "An unexpected error occurred while retrieving monthly summary.",
                instance: HttpContext.Request.Path,
                traceId: HttpContext.TraceIdentifier
            ));
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
