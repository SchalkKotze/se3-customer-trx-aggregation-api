namespace aggregate_api.Application.Domain.Constants;

public static class LoggingMessages
{
    public static string Exception(string parameterOne, string parameterTwo)
    {
        return string.Format("An error has occured in: {0} - {1}", parameterOne, parameterTwo);
    }

    public static string Executing(string parameterOne, string parameterTwo)
    {
        return string.Format("Executing: {0} - {1}", parameterOne, parameterTwo);
    }

    public static string Stopwatch(string parameterOne, string parameterTwo, TimeSpan timespan)
    {
        return string.Format("{0} - {1} executed in: {2} minutes, {3} seconds, {4} milliseconds {5} nanoseconds",
            parameterOne, parameterTwo, timespan.Minutes, timespan.Seconds, timespan.Milliseconds, timespan.Nanoseconds);
    }
}