namespace SoccerPro.Domain;

public record Error(string Message, string? Code = null);
public class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }
    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    public static Result Success() => new(true, null);
    public static Result Failure(string message, string? code = null) => new(false, new Error(message, code));
}
public class Result<T> : Result
{
    public T? Value { get; }
    private Result(T value) : base(true, null) => Value = value;
    private Result(Error error) : base(false, error) { }
    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(string message, string? code = null) => new(new Error(message, code));
}
