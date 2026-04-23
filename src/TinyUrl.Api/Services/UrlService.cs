using Microsoft.EntityFrameworkCore;
using TinyUrl.Api.Data;
using TinyUrl.Api.Models;

namespace TinyUrl.Api.Services;

public class UrlService : IUrlService
{
    private readonly AppDbContext _context;
    private readonly IShortCodeGenerator _codeGenerator;
    private readonly ILogger<UrlService> _logger;

    public UrlService(AppDbContext context, IShortCodeGenerator codeGenerator, ILogger<UrlService> logger)
    {
        _context = context;
        _codeGenerator = codeGenerator;
        _logger = logger;
    }

    public async Task<ShortUrl> CreateAsync(string originalUrl, bool isPrivate)
    {
        var shortCode = _codeGenerator.Generate();
        
        // Ensure uniqueness
        while (await _context.ShortUrls.AnyAsync(u => u.ShortCode == shortCode))
        {
            shortCode = _codeGenerator.Generate();
        }

        var shortUrl = new ShortUrl
        {
            ShortCode = shortCode,
            OriginalUrl = originalUrl,
            IsPrivate = isPrivate,
            CreatedAt = DateTime.UtcNow
        };

        _context.ShortUrls.Add(shortUrl);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Created short URL: {ShortCode} -> {OriginalUrl}", shortCode, originalUrl);
        return shortUrl;
    }

    public async Task<IEnumerable<ShortUrl>> GetPublicUrlsAsync(string? search = null)
    {
        var query = _context.ShortUrls.Where(u => !u.IsPrivate);
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => 
                u.OriginalUrl.Contains(search) || 
                u.ShortCode.Contains(search));
        }

        return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }

    public async Task<ShortUrl?> GetByShortCodeAsync(string shortCode)
    {
        return await _context.ShortUrls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);
    }

    public async Task<bool> DeleteAsync(string shortCode)
    {
        var url = await _context.ShortUrls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);
        if (url is null) return false;

        _context.ShortUrls.Remove(url);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted short URL: {ShortCode}", shortCode);
        return true;
    }

    public async Task IncrementClickAsync(string shortCode)
    {
        await _context.ShortUrls
            .Where(u => u.ShortCode == shortCode)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.ClickCount, u => u.ClickCount + 1));
    }

    public async Task DeleteAllAsync()
    {
        await _context.ShortUrls.ExecuteDeleteAsync();
        _logger.LogInformation("Deleted all short URLs");
    }
}
