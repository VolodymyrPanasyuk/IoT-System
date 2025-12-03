using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IoT_System.Infrastructure.Services;

public class HealthCheckBackgroundService : BackgroundService
{
    private readonly ILogger<HealthCheckBackgroundService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string? _healthCheckUrl;
    private readonly TimeSpan _interval;

    public HealthCheckBackgroundService(
        ILogger<HealthCheckBackgroundService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _healthCheckUrl = configuration["HealthCheck:Url"];

        var intervalMinutes = configuration.GetValue("HealthCheck:IntervalMinutes", 10);
        _interval = TimeSpan.FromMinutes(intervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Skip health check if the url is not configured
        if (string.IsNullOrWhiteSpace(_healthCheckUrl))
        {
            _logger.LogWarning("Health check url is not configured, skipping health check");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformHealthCheckAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while performing health check");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task PerformHealthCheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("HealthCheck");
            var response = await httpClient.GetAsync(_healthCheckUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Health check successful: {StatusCode}", response.StatusCode);
            }
            else
            {
                _logger.LogWarning("Health check returned non-success status: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform health check to {Url}", _healthCheckUrl);
        }
    }
}