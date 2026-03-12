using System.Data;
using aggregate_api.Application.Interfaces;
using Npgsql;
using StackExchange.Redis;
using System.Text.Json;

namespace aggregate_api.Infrastructure.ExternalData;

public class BankSourceDatabase : ITransactionSource
{
    private readonly string _connectionString;
    private readonly IDatabase _redisCache;

    public BankSourceDatabase(IConnectionMultiplexer redis)
    {
        _connectionString = Environment.GetEnvironmentVariable("POSTGRE_CONNECTION_STRING") 
                            ?? throw new InvalidOperationException("PostgreSQL connection string is not set in environment variables.");
        _redisCache = redis.GetDatabase();
    }

    internal static List<BankSourceRawTransaction>? DeserializeCachedTransactions(string cachedData)
    {
        return JsonSerializer.Deserialize<List<BankSourceRawTransaction>>(cachedData);
    }

    public async Task<IEnumerable<RawTransaction>> GettransactionsAsync(string customerID, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var cacheKey = $"BankTransactions:{customerID}";

        var cachedData = await _redisCache.StringGetAsync(cacheKey);
        if (!cachedData.IsNullOrEmpty)
        {
            try
            {
                var cachedTransactions = DeserializeCachedTransactions(cachedData!);
                if (cachedTransactions is not null)
                {
                    return cachedTransactions;
                }
            }
            catch (JsonException)
            {
                await _redisCache.KeyDeleteAsync(cacheKey);
            }
        }

        var transactions = new List<BankSourceRawTransaction>();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync(token);

            using (var command = new NpgsqlCommand("SELECT * FROM BankTransactions WHERE CustomerID = @CustomerID", connection))
            {
                command.Parameters.Add(new NpgsqlParameter("@CustomerID", DbType.String) { Value = customerID });

                using (var reader = await command.ExecuteReaderAsync(token))
                {
                    while (await reader.ReadAsync(token))
                    {
                        transactions.Add(new BankSourceRawTransaction
                        {
                            Source = reader["Source"].ToString(),
                            CustomerID = reader["CustomerID"].ToString(),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            Description = reader["Description"].ToString(),
                            TransactiopnDate = Convert.ToDateTime(reader["TransactiopnDate"]),
                            BankTransactionID = reader["BankTransactionID"].ToString(),
                        });
                    }
                }
            }
        }

        var serializedData = JsonSerializer.Serialize(transactions);
        await _redisCache.StringSetAsync(cacheKey, serializedData, TimeSpan.FromMinutes(10));

        return transactions;
    }
}