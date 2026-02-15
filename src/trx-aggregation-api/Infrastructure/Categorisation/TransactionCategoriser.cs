using aggregate_api.Application.Domain.Models;
using aggregate_api.Application.Interfaces;
using aggregate_api.Application.Domain.Constants;
using aggregate_api.Application.Domain.Enums;
using TransactionCategory = aggregate_api.Application.Domain.Enums.TransactionCategory;

namespace aggregate_api.Application.Infrastructure.Categorisation;

public class TransactionCategoriser : ITransactionCategoriser
{
    public List<Transaction> Categorise(IEnumerable<Transaction> transactions)
    {
        foreach (var transaction in transactions)
        {
            transaction.Category = (DetermineCategory(transaction));
        }

        return transactions.ToList();
    }

    private static TransactionCategory DetermineCategory(Transaction transaction)
    {
        var desc = transaction.Description.ToLowerInvariant();
        
        if (CategoryLookups.GroceryKeywords.Any(g => desc.Contains(g)))
        {
            return TransactionCategory.Groceries;
        }
        if (CategoryLookups.EntertainmentKeywords.Any(g => desc.Contains(g)))
        {
            return TransactionCategory.Entertainment;
        }

        return TransactionCategory.Other;
    }
}