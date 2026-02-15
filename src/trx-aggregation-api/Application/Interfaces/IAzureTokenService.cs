using aggregate_api.Application.Dtos;
using aggregate_api.Application.Domain.Models;
using aggregate_api.Application.Domain.Responses;

namespace aggregate_api.Application.Services.Contracts;

public interface IAzureTokenService
{
    Task<ResponseModel<TokenResponse>> RequestAccessTokenAsync(GenerateTokenDto generateTokenDto, CancellationToken token);
}