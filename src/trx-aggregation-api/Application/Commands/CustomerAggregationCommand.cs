using System.Runtime.InteropServices.JavaScript;
using aggregate_api.Application.Domain.Enums;

namespace aggregate_api.Application.Dtos;

public class CustomerAggregationCommand
{
    public string CorrelationId { get; set; } = string.Empty;
    public string SourceSystem { get; set; } = string.Empty;
    public string AppId { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public string UserRoles { get; set; } = string.Empty;
    public IReadOnlyCollection<string> CustomerIds { get; init; } = Array.Empty<string>();
    public DateTime EventTriggerDate { get; set; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }

}