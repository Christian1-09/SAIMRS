namespace SAIMRS.Application.Common;

public static class PasswordPolicy
{
    public const int MinLength = 8;

    /// <summary>Returns an error message, or null when the password is acceptable.</summary>
    public static string? Validate(string? password, string? confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "Password is required.";
        if (password.Length < MinLength)
            return $"Password must be at least {MinLength} characters.";
        if (password != confirmPassword)
            return "Passwords do not match.";
        return null;
    }

    /// <summary>Answers are compared ignoring case and extra spaces.</summary>
    public static string NormalizeAnswer(string answer) =>
        string.Join(' ', answer.Trim().ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
