
using aggregate_api.Application.Interfaces;
//using aggregate_api.Application.Domain.Transactions;
using aggregate_api.Application.Domain.Models;
using aggregate_api.Infrastructure;

namespace aggregate_api.Application.Infrastructure.Normalisation;

public class TransactionNormaliser : ITransactionNormaliser
{
    public Transaction Normalise(object sourceTransaction, string sourceName)
    {
        return sourceName switch
        {
            "BX" => NormaliseBank((BankSourceRawTransaction)sourceTransaction),
            "CX" => NormaliseCredit((CreditRawTransactions)sourceTransaction),
            _ => throw new NotSupportedException($"Transaction Type {sourceName} is not supported")
        };
    }

    private static Transaction NormaliseBank(BankSourceRawTransaction trx)
    {
        return new ()
        {
            TransactionID = trx.BankTransactionID,
            CustomerID = trx.CustomerID,
            Amount = trx.Amount,
            Currency = "ZAR",
            TransactionDate = trx.TransactiopnDate,
            Description = trx.Description,
            Source = trx.Source
        };
    }
    private static Transaction NormaliseCredit(CreditRawTransactions trx)
    {
        return new ()
        {
            TransactionID = trx.CreditGuid.ToString(),
            CustomerID = trx.Merchant,
            Amount = trx.AmountCents,
            Currency = "ZAR",
            TransactionDate = DateTime.Parse(trx.TimeStamp),
            Description = trx.Description,
            Source = trx.Source
        };
    }
}
