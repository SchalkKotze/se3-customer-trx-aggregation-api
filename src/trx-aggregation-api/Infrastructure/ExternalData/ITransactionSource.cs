using aggregate_api.Infrastructure;

namespace aggregate_api.Application.Interfaces;

public interface ITransactionSource
{
    Task<IEnumerable<RawTransaction>> GettransactionsAsync(string customerID,CancellationToken token);

}