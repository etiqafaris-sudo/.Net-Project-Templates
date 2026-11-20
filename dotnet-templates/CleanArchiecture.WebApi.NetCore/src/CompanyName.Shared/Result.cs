namespace CompanyName.Shared;

public class Result
{
    protected Result(bool isSuccess, IReadOnlyList<string>? errors)
    {
        if (isSuccess && errors is { Count: > 0 })
        {
            throw new InvalidOperationException("A successful result cannot have errors.");
        }

        if (!isSuccess && (errors is null || errors.Count == 0))
        {
            throw new InvalidOperationException("A failed result must have at least one error.");
        }

        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<string>? Errors { get; }
    public string? Error => Errors is { Count: > 0 } ? Errors[0] : null;

    public static Result Success() => new(true, null);

    public static Result Failure(string error) => new(false, [error]);

    public static Result Failure(IReadOnlyList<string> errors) => new(false, errors);

    public static Result Failure(params string[] errors) => new(false, errors);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);

    public static Result<T> Failure<T>(IReadOnlyList<string> errors) => Result<T>.Failure(errors);

    public static Result<T> Failure<T>(params string[] errors) => Result<T>.Failure(errors);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base(true, null)
    {
        _value = value;
    }

    private Result(IReadOnlyList<string> errors) : base(false, errors)
    {
        _value = default;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static Result<T> Success(T value) => new(value);

    public new static Result<T> Failure(string error) => new([error]);

    public new static Result<T> Failure(IReadOnlyList<string> errors) => new(errors);

    public new static Result<T> Failure(params string[] errors) => new(errors);
}
