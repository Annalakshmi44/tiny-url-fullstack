using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace TinyUrl.Functions;

public class CleanupFunction
{
    private readonly ILogger<CleanupFunction> _logger;

    public CleanupFunction(ILogger<CleanupFunction> logger)
    {
        _logger = logger;
    }

    [Function("CleanupUrls")]
    public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("Cleanup function started at: {Time}", DateTime.UtcNow);

        var connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogError("SQL connection string not configured");
            return;
        }

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("DELETE FROM ShortUrls", connection);
            var rowsDeleted = await command.ExecuteNonQueryAsync();

            _logger.LogInformation("Deleted {Count} URLs", rowsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup");
            throw;
        }
    }
}
