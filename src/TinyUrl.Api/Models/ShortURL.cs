namespace TinyUrl.Api.Models;

public class ShortUrl
{
    public int Id { get; set; }
    public string ShortCode { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public int ClickCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
