
using System.Text.Json.Serialization;
using aggregate_api.Application.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace aggregate_api.Application.Domain.Requests.v1;

public class SendAggregateRequest
{
    public List<string> CustomerIds { get; init; } = new List<string>();
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    [Newtonsoft.Json.JsonConverter(typeof(JsonStringEnumConverter))]
    [SwaggerSchema("Optional SourceSystem filter. Allowed values : BX,CX,IX")]
    public SourceSystem? SourceSystem { get; init; }
}
