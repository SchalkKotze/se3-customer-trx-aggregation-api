namespace aggregate_api.Application.Domain.Constants;

public static class ValidationMessages
{
    public const string NullCheck = "{0} value cannot be null";
    public const string EmptyCheck = "{0} value cannot be empty";
    public const string MinimumLengthCheck = "{0} length cannot be less than {1} characters long";
    public const string MaximumLengthCheck = "{0} length cannot be greater than {1} characters long";
    public const string MustContainCheck = "{0} value must contain one of the following: {1}";
    public const string MustIncludeCheck = "{0} must be included with: {1}";
    public const string Base64Check = "{0} must contain a valid base64 string";
}