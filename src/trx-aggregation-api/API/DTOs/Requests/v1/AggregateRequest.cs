using aggregate_api.Application.Domain.Models;

namespace aggregate_api.Application.Domain.Requests.v1;

public class SendAggregateRequest
{
    public List<string> CustomerIds { get; init; } = new List<string>();
}