namespace SAIMRS.Application.Common;

/// <summary>
/// Outcome of a use case. Services return a Result instead of throwing,
/// so the UI can simply show <see cref="Error"/> when something fails.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(string error) => new(default, false, error);
}

public sealed class Result<T> : Result
{
    internal Result(T? value, bool isSuccess, string? error) : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>The value when <see cref="Result.IsSuccess"/> is true.</summary>
    public T? Value { get; }
}
