using aggregate_api.Application.Domain.Models;
using aggregate_api.Application.Domain.Responses;
using aggregate_api.Application.Dtos;

namespace aggregate_api.Application.Interfaces;

public interface IAggregateService
{
    Task<ResponseModel<List<AggregatedCustomerTransactionsDto>>> AggregateClientsAsync(CustomerAggregationCommand customerAggregationCommand, CancellationToken token);
}


