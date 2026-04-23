using Microsoft.EntityFrameworkCore;
using Serilog;
using TinyUrl.Api.Data;
using TinyUrl.Api.Models;
using TinyUrl.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        path: builder.Configuration["Logging:FilePath"] ?? "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

builder.Host.UseSerilog();

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useAzureSql = builder.Configuration.GetValue<bool>("UseAzureSql");

if (useAzureSql && !string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite("Data Source=tinyurl.db"));
}

// Services
builder.Services.AddScoped<IShortCodeGenerator, ShortCodeGenerator>();
builder.Services.AddScoped<IUrlService, UrlService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TinyURL API", Version = "v1" });
});

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();

var baseUrl = builder.Configuration["BaseUrl"] ?? "[localhost](http://localhost:5000)";

// API Endpoints
var api = app.MapGroup("/api/urls");

api.MapPost("/", async (CreateUrlRequest request, IUrlService service) =>
{
    if (string.IsNullOrWhiteSpace(request.Url))
        return Results.BadRequest(new ApiResponse<object>(false, null, "URL is required"));

    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
        return Results.BadRequest(new ApiResponse<object>(false, null, "Invalid URL format"));

    var shortUrl = await service.CreateAsync(request.Url, request.IsPrivate);
    var response = new UrlResponse(
        shortUrl.Id,
        shortUrl.ShortCode,
        $"{baseUrl}/{shortUrl.ShortCode}",
        shortUrl.OriginalUrl,
        shortUrl.IsPrivate,
        shortUrl.ClickCount,
        shortUrl.CreatedAt);

    return Results.Created($"/api/urls/{shortUrl.ShortCode}", new ApiResponse<UrlResponse>(true, response));
})
.WithName("CreateShortUrl")
.Produces<ApiResponse<UrlResponse>>(201)
.Produces<ApiResponse<object>>(400);

api.MapGet("/", async (string? search, IUrlService service) =>
{
    var urls = await service.GetPublicUrlsAsync(search);
    var response = urls.Select(u => new UrlResponse(
        u.Id,
        u.ShortCode,
        $"{baseUrl}/{u.ShortCode}",
        u.OriginalUrl,
        u.IsPrivate,
        u.ClickCount,
        u.CreatedAt));

    return Results.Ok(new ApiResponse<IEnumerable<UrlResponse>>(true, response));
})
.WithName("GetPublicUrls")
.Produces<ApiResponse<IEnumerable<UrlResponse>>>();

api.MapGet("/{shortCode}", async (string shortCode, IUrlService service) =>
{
    var url = await service.GetByShortCodeAsync(shortCode);
    if (url is null)
        return Results.NotFound(new ApiResponse<object>(false, null, "URL not found"));

    var response = new UrlResponse(
        url.Id,
        url.ShortCode,
        $"{baseUrl}/{url.ShortCode}",
        url.OriginalUrl,
        url.IsPrivate,
        url.ClickCount,
        url.CreatedAt);

    return Results.Ok(new ApiResponse<UrlResponse>(true, response));
})
.WithName("GetUrlByCode")
.Produces<ApiResponse<UrlResponse>>()
.Produces<ApiResponse<object>>(404);

api.MapDelete("/{shortCode}", async (string shortCode, IUrlService service) =>
{
    var deleted = await service.DeleteAsync(shortCode);
    return deleted 
        ? Results.Ok(new ApiResponse<object>(true, null)) 
        : Results.NotFound(new ApiResponse<object>(false, null, "URL not found"));
})
.WithName("DeleteUrl")
.Produces<ApiResponse<object>>()
.Produces<ApiResponse<object>>(404);

// Redirect endpoint
app.MapGet("/{shortCode}", async (string shortCode, IUrlService service) =>
{
    var url = await service.GetByShortCodeAsync(shortCode);
    if (url is null)
        return Results.NotFound("Short URL not found");

    await service.IncrementClickAsync(shortCode);
    return Results.Redirect(url.OriginalUrl, permanent: false);
})
.WithName("RedirectToOriginal")
.ExcludeFromDescription();

// Admin endpoint for cleanup (protected by secret token)
app.MapDelete("/admin/urls/all", async (HttpContext context, IUrlService service, IConfiguration config) =>
{
    var secretToken = config["SecretToken"];
    var providedToken = context.Request.Headers["X-Secret-Token"].FirstOrDefault();

    if (string.IsNullOrEmpty(secretToken) || providedToken != secretToken)
        return Results.Unauthorized();

    await service.DeleteAllAsync();
    return Results.Ok(new ApiResponse<object>(true, null));
})
.WithName("DeleteAllUrls")
.ExcludeFromDescription();

app.Run();
