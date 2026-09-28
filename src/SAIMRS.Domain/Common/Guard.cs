namespace SAIMRS.Domain.Common;

/// <summary>Small guard helpers used by entity constructors and methods.</summary>
internal static class Guard
{
    public static string Required(string? value, string fieldName, int maxLength = 200)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new DomainException($"{fieldName} is required.");
        if (trimmed.Length > maxLength)
            throw new DomainException($"{fieldName} must be {maxLength} characters or fewer.");
        return trimmed;
    }

    public static string? Optional(string? value, string fieldName, int maxLength = 500)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        if (trimmed.Length > maxLength)
            throw new DomainException($"{fieldName} must be {maxLength} characters or fewer.");
        return trimmed;
    }

    public static decimal NotNegative(decimal value, string fieldName)
    {
        if (value < 0)
            throw new DomainException($"{fieldName} cannot be negative.");
        return value;
    }

    public static decimal Positive(decimal value, string fieldName)
    {
        if (value <= 0)
            throw new DomainException($"{fieldName} must be greater than zero.");
        return value;
    }
}
