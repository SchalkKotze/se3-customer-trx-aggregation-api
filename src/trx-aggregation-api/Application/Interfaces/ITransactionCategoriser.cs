using aggregate_api.Application.Domain.Models;

namespace aggregate_api.Application.Interfaces;

public interface ITransactionCategoriser
{
    List<Transaction> Categorise(IEnumerable<Transaction> transactions);
}