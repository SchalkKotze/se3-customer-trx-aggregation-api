namespace aggregate_api.Infrastructure;

public abstract class RawTransaction
{
    public string Source { get; init; } = default!;
}