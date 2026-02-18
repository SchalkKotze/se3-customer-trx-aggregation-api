using aggregate_api.Application.Interfaces;

namespace aggregate_api.Infrastructure.ExternalData;

public class BankSource : ITransactionSource
{
    public async Task<IEnumerable<RawTransaction>> GettransactionsAsync(string customerID,CancellationToken token)
    {
        
        //Faking Bank transaction from A Database Source
        
        token.ThrowIfCancellationRequested();
        
        return new List<RawTransaction>
        {
            new BankSourceRawTransaction
            {
                Source = "BX",
                CustomerID = "1",
                Amount = 130.99m,
                Description = "Spar",
                TransactiopnDate = DateTime.UtcNow.Add(TimeSpan.FromDays(-1)),
                BankTransactionID = "B01",

            },
            new BankSourceRawTransaction
            {
                Source = "BX",
                CustomerID = "2",
                Amount = 1270.34m,
                Description = "FoodLovers",
                TransactiopnDate = DateTime.UtcNow.Add(TimeSpan.FromDays(-120)),
                BankTransactionID = "B03",

            }
        };
    }
}