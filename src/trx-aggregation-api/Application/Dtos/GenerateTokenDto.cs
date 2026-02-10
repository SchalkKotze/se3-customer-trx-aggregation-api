namespace aggregate_api.Application.Dtos;

public class GenerateTokenDto
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}