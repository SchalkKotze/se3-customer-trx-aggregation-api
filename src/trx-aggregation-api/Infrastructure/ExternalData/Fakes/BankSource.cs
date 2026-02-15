using aggregate_api.Application.Interfaces;

namespace aggregate_api.Infrastructure.ExternalData;

public class BankSource : ITransactionSource
{
    public async Task<IEnumerable<RawTransaction>> GettransactionsAsync(string customerID)
    {
        return new List<RawTransaction>
        {
            new BankSourceRawTransaction
            {
                Source = "BX",
                CustomerID = customerID,
                Amount = 130.99m,
                Descriptiopn = "Spar",
                TransactiopnDate = DateTime.UtcNow.Add(TimeSpan.FromDays(-1)),
                BankTransactionID = "B01",

            },
            new BankSourceRawTransaction
            {
                Source = "BX",
                CustomerID = customerID,
                Amount = 1270.34m,
                Descriptiopn = "FoodLovers",
                TransactiopnDate = DateTime.UtcNow.Add(TimeSpan.FromDays(-120)),
                BankTransactionID = "B03",

            }
        };
    }
}