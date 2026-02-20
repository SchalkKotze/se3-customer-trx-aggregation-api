namespace aggregate_api.Application.Dtos;

public class CustomerMonthlySummaryDto
{
    public string CustomerID { get; set; } = default!;
    public int Year { get; set; }
    public int Month { get; set; }

    public decimal TotalInflow { get; set; }
    public decimal TotalOutflow { get; set; }
    public decimal NetAmount { get; set; }

    public int TransactionCount { get; set; }
}