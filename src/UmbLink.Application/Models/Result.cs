namespace UmbLink.Application.Models;

public class Result<T>
{
    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public LimitExceededError? LimitError { get; }

    private Result(T value) { Value = value; IsSuccess = true; }
    private Result(string error, LimitExceededError? limitError = null)
    {
        Error = error;
        IsSuccess = false;
        LimitError = limitError;
    }

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(string error) => new(error);
    public static Result<T> LimitExceeded(LimitExceededError err) => new(err.Message, err);
}
