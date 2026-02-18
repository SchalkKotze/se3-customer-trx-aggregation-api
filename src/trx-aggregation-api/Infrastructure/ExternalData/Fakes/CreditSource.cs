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
                Merchant = "1",
                Description = "TicketPro",
                TimeStamp = DateTime.UtcNow.Date.Add(TimeSpan.FromDays(-60)).ToString("o")
            },
            new CreditRawTransactions
            {
                Source = "CX",
                CreditGuid = Guid.NewGuid(),
                AccountID = "A210020",
                AmountCents = 13000/100,
                Merchant = "2",
                Description = "Spar",
                TimeStamp = DateTime.UtcNow.Date.Add(TimeSpan.FromDays(-60)).ToString("o")
            },
        };
    }
}