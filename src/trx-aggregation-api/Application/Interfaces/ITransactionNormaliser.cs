using aggregate_api.Application.Domain.Models;

namespace aggregate_api.Application.Interfaces;

public interface ITransactionNormaliser
{
    Transaction Normalise(object sourceTransaction, string sourceName);
}