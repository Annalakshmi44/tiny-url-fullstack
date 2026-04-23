namespace TinyUrl.Api.Models;

public record CreateUrlRequest(string Url, bool IsPrivate = false);
public record UrlResponse(int Id, string ShortCode, string ShortUrl, string OriginalUrl, bool IsPrivate, int ClickCount, DateTime CreatedAt);
public record ApiResponse<T>(bool Success, T? Data, string? Error = null);
