using TinyUrl.Api.Models;

namespace TinyUrl.Api.Services;

public interface IUrlService
{
    Task<ShortUrl> CreateAsync(string originalUrl, bool isPrivate);
    Task<IEnumerable<ShortUrl>> GetPublicUrlsAsync(string? search = null);
    Task<ShortUrl?> GetByShortCodeAsync(string shortCode);
    Task<bool> DeleteAsync(string shortCode);
    Task IncrementClickAsync(string shortCode);
    Task DeleteAllAsync();
}
