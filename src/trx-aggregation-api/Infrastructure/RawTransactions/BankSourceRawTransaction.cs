namespace aggregate_api.Infrastructure;

public class BankSourceRawTransaction  : RawTransaction
{
    public string BankTransactionID { get; init; } = default!;
    public string CustomerID { get; init; } = default!;
    public decimal Amount { get; init; } 
    public string Description { get; init; } = default!;
    public DateTime TransactiopnDate { get; init; } 
}