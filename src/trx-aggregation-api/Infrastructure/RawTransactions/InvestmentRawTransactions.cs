namespace aggregate_api.Infrastructure;

public class InvestmentRawTransactions  : RawTransaction
{
    
    public Guid CreditGuid { get; init; }
    public string AccountID { get; init; } = default!;
    public long AmountCents { get; init; } 
    public string Merchant { get; init; } = default!;
    public string TimeStamp { get; init; }
    public string Description { get; init; }

}