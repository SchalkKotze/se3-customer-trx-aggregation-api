namespace aggregate_api.Application.Domain.Models;

public class SpendByCategoryDto
{
    public string CustomerId { get; set; } = default!;
    public Enums.TransactionCategory Category { get; set; } = default!;
    public decimal TotalSpent { get; set; }
    public string Currency { get; set; } = default!;
}