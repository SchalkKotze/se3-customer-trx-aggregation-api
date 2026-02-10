using aggregate_api.Application.Dtos;

namespace aggregate_api.Core.AutoMapper.Extentions;

public static class MessageDtoExtention
{
    public static CustomerAggregationCommand WithAppId(this CustomerAggregationCommand customerAggregationCommand, string appId)
    {
        customerAggregationCommand.AppId = appId;
        return customerAggregationCommand;
    }
    
    public static CustomerAggregationCommand WithAppDisplayName(this CustomerAggregationCommand customerAggregationCommand, string appDisplayName)
    {
        customerAggregationCommand.AppName = appDisplayName;
        return customerAggregationCommand;
    }

    public static CustomerAggregationCommand WithUserRoles(this CustomerAggregationCommand customerAggregationCommand, string userRoles)
    {
        customerAggregationCommand.UserRoles = userRoles;
        return customerAggregationCommand;
    }

    public static CustomerAggregationCommand WithCorrelationId(this CustomerAggregationCommand customerAggregationCommand, string? correlationId)
    {
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            customerAggregationCommand.CorrelationId = correlationId;
        }
        else
        {
            customerAggregationCommand.CorrelationId = Guid.NewGuid().ToString();
        }

        return customerAggregationCommand;
    }
    
    public static CustomerAggregationCommand WithSourceSystem(this CustomerAggregationCommand customerAggregationCommand, string? sourceSystem)
    {
        if (!string.IsNullOrWhiteSpace(sourceSystem))
        {
            customerAggregationCommand.SourceSystem = sourceSystem;
        }
        else
        {
            customerAggregationCommand.SourceSystem = "NO_SOURCE_SYSTEM";
        }
        
        return customerAggregationCommand;
    }
}