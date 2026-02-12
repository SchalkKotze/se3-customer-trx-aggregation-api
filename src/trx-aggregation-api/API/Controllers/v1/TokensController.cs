using aggregate_api.Application.Domain.Constants;
using aggregate_api.Application.Dtos;
using aggregate_api.Application.Domain.Requests.v1;
using aggregate_api.Application.Services.Contracts;
using aggregate_api.Core.Swagger.Filters;
using aggregate_api.Infrastructure.Contracts;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace aggregate_api.Application.Controllers.v1;

[Route("api/v1/tokens")]
[ApiController]
public class TokensController : ControllerBase
{
    private readonly ILoggingService _loggingService;
    private readonly IAzureTokenService _azureTokenService;
    private readonly IMapper _mapper;

    public TokensController(ILoggingService loggingService,
                            IAzureTokenService azureTokenService,
                            IMapper mapper)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _azureTokenService = azureTokenService ?? throw new ArgumentNullException(nameof(azureTokenService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    
    [HttpPost]
    [SwaggerOperationFilter(typeof(SwaggerResponseFilter))]
    [ApiExplorerSettings(GroupName = "v1")]
    public async Task<IActionResult> GenerateAsync(GenerateTokenRequest request, CancellationToken token)
    {
        try
        {
            var generateTokenDto = _mapper.Map<GenerateTokenDto>(request);
            var responseModel = await _azureTokenService.RequestAccessTokenAsync(generateTokenDto, token);
            
            if (responseModel.IsValid)
            {
                return Ok(responseModel.Data);
            }

            return BadRequest(responseModel.Errors);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(LoggingMessages.Exception("TokensController", "GenerateAsync"), ex);
            
            return StatusCode(500);
        }
    }
}