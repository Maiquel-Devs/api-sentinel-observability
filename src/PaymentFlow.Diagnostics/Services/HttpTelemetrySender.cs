using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentFlow.Diagnostics.Models;
using PaymentFlow.Diagnostics.Options;

namespace PaymentFlow.Diagnostics.Services;

public class HttpTelemetrySender : ITelemetrySender
{
    private readonly HttpClient _httpClient;
    private readonly SentinelOptions _options;
    private readonly ILogger<HttpTelemetrySender> _logger;

    public HttpTelemetrySender(
        HttpClient httpClient,
        IOptions<SentinelOptions> options,
        ILogger<HttpTelemetrySender> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(ErrorTelemetryPayload payload, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.WebhookUrl))
        {
            return;
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync(_options.WebhookUrl, payload, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            // Falha na telemetria não pode derrubar a requisição original
            _logger.LogError(ex, "Falha ao despachar telemetria para o n8n.");
        }
    }
}