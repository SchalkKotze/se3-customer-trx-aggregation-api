namespace aggregate_api.Application.Domain.Responses;

public class TokenResponse
{
    public string TokenType { get; set; } = string.Empty;
    public DateTimeOffset ExpiresOn { get; set; }
    public string AccessToken { get; set; } = string.Empty;
}