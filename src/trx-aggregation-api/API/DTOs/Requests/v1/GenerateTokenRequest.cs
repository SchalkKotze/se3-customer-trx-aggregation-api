namespace aggregate_api.Application.Domain.Requests.v1;

public class GenerateTokenRequest
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}