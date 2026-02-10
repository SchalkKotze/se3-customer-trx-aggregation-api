namespace aggregate_api.Application.Dtos;

public class AggregatedCustomerTransactionsDto
{
    public string CustomerID { get; set; } = string.Empty;
    public List<AggregatedCategoryResultsDtos> CategoryAggregates { get; set; } = new();
    public decimal TotalBalance => CategoryAggregates.Sum(c => c.Amount);
}