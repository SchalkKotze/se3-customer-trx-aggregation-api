using aggregate_api.Application.Interfaces;

namespace aggregate_api.Infrastructure.ExternalData;

public class CreditSource : ITransactionSource
{
    public  async Task<IEnumerable<RawTransaction>> GettransactionsAsync(string customerID)
    {
        return new List<RawTransaction>
        {
            new CreditRawTransactions
            {
                Source = "CX",
                CreditGuid = Guid.NewGuid(),
                AccountID = "A210020",
                AmountCents = 13000/100,
                Merchant = customerID,
                Description = "TicketPro",
                TimeStamp = DateTime.UtcNow.Date.Add(TimeSpan.FromDays(-60)).ToString("o")
            }
        };
    }
}