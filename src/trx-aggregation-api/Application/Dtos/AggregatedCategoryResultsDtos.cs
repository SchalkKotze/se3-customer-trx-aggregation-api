using aggregate_api.Application.Domain.Enums;

namespace aggregate_api.Application.Dtos;

public class AggregatedCategoryResultsDtos
{
    public TransactionCategory Category { get; set; }
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
}