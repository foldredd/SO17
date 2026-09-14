namespace SO17.Application.Common;

public sealed class AppException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public static AppException NotFound() => new(404, "Запис не знайдено.");
    public static AppException Conflict(string message) => new(409, message);
}
