using aggregate_api.Application.Interfaces;

namespace aggregate_api.Infrastructure.ExternalData;

public class InvestmentSource  : ITransactionSource
{
    public Task<IEnumerable<RawTransaction>> GettransactionsAsync(string customerID)
    {
        throw new NotImplementedException();
    }
}