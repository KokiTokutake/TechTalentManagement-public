namespace TechTalentManagement.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public int StatusCode { get; }

    public Result(bool isSuccess, T? value, string? error, int statusCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        StatusCode = statusCode;
    }

    public static Result<T> Ok(T value) => new(true, value, null, 200);
    public static Result<T> Created(T value) => new(true, value, null, 201);
    public static Result<T> NotFound(string error) => new(false, default, error, 404);
    public static Result<T> Forbidden(string error) => new(false, default, error, 403);
    public static Result<T> Conflict(string error) => new(false, default, error, 409);
    public static Result<T> BadRequest(string error) => new(false, default, error, 400);
}
