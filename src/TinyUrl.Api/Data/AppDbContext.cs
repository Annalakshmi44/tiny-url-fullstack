using Microsoft.EntityFrameworkCore;
using TinyUrl.Api.Models;

namespace TinyUrl.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortUrl>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ShortCode).IsUnique();
            entity.Property(e => e.ShortCode).HasMaxLength(10).IsRequired();
            entity.Property(e => e.OriginalUrl).HasMaxLength(2048).IsRequired();
        });
    }
}
