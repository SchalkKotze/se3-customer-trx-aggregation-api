namespace aggregate_api.Application.Domain.Models;

public class Transaction
{
    public string TransactionID { get; init; } = default!;
    public string CustomerID{ get; init; } = default!;
    public decimal Amount{ get; init; } 
    public string Currency{ get; init; } = default!;
    public DateTime TransactionDate{ get; init; }
    public string Source{ get; init; } = default!;
    public string Description { get; init; }
    public Enums.TransactionCategory Category { get; set; } = Enums.TransactionCategory.Other;

}