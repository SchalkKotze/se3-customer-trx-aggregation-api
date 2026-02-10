using System.Text.RegularExpressions;

namespace aggregate_api.Application.Domain.Constants;

public static class RegexProvider
{
    public static Regex ValidEmailRegex { get; } =
        new(@"^[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
}