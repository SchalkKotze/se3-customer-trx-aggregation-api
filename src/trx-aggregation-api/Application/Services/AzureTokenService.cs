using System.Diagnostics;
using System.Globalization;
using aggregate_api.Application.Domain.Constants;
using aggregate_api.Application.Dtos;
using aggregate_api.Application.Domain.Models;
using aggregate_api.Application.Domain.Responses;
using aggregate_api.Application.Services.Contracts;
using aggregate_api.Core.FluentValidators.Services.Contracts;
using aggregate_api.Infrastructure.Contracts;
using FluentValidation;
using Microsoft.Identity.Client;

namespace aggregate_api.Application.Services;

public class AzureTokenService : IAzureTokenService
{
    private readonly ILoggingService _loggingService;
    private readonly IEnvironmentService _environmentService;
    private readonly IFluentValidationService _fluentValidationService;
    private readonly IValidator<GenerateTokenDto> _generateTokenDtoValidator;
    

    public AzureTokenService(ILoggingService loggingService,
                             IEnvironmentService environmentService,
                             IFluentValidationService fluentValidationService,
                             IValidator<GenerateTokenDto> generateTokenDtoValidator
                             )
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _environmentService = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
        _fluentValidationService = fluentValidationService ?? throw new ArgumentNullException(nameof(fluentValidationService));
        _generateTokenDtoValidator = generateTokenDtoValidator ?? throw new ArgumentNullException(nameof(generateTokenDtoValidator));
       
    }

    public async Task<ResponseModel<TokenResponse>> RequestAccessTokenAsync(GenerateTokenDto generateTokenDto, CancellationToken token)
    {
        var r = new ResponseModel<TokenResponse>(new TokenResponse());
        
        _loggingService.LogTrace(LoggingMessages.Executing("AzureTokenService", "RequestAccessTokenAsync"));
        
        r.MergeResponses(_fluentValidationService.ValidateAggregateCommand(generateTokenDto, _generateTokenDtoValidator));
        
        if (r.IsValid && !token.IsCancellationRequested)
        {
            var authenticationResult = await AcquireClientCredentialTokenAsync(generateTokenDto);

            if (!string.IsNullOrWhiteSpace(authenticationResult.AccessToken))
            {
                r.Data.TokenType = authenticationResult.TokenType;
                r.Data.ExpiresOn = authenticationResult.ExpiresOn.AddHours(2);
                r.Data.AccessToken = authenticationResult.AccessToken;
            }
        }
        
        return r;
    }
    
    private async Task<AuthenticationResult> AcquireClientCredentialTokenAsync(GenerateTokenDto generateTokenDto)
    {
        _loggingService.LogTrace(LoggingMessages.Executing("AzureTokenService", "AcquireClientCredentialTokenAsync"));
        
        var confidentialClientApplicationBuilder = ConfidentialClientApplicationBuilder
            .Create(generateTokenDto.ClientId)
            .WithClientSecret(generateTokenDto.ClientSecret)
            .WithAuthority(new Uri(String.Format(CultureInfo.InvariantCulture, _environmentService.InstanceUrl + "{0}", _environmentService.TenantId)))
            .Build();
        
        var ResourceIds = new[] { String.Format("{0}/.default", _environmentService.ResourceId) };
        
        var authenticationResult = await confidentialClientApplicationBuilder
            .AcquireTokenForClient(ResourceIds)
            .ExecuteAsync();
        
        return authenticationResult;
    }
}