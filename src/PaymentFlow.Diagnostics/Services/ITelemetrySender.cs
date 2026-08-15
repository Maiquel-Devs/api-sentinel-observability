using PaymentFlow.Diagnostics.Models;

namespace PaymentFlow.Diagnostics.Services;

/// <summary>
/// Define o contrato para o serviço responsável por despachar os dados de telemetria 
/// de incidentes para um pipeline de observabilidade externo (ex: Webhook do n8n).
/// </summary>
public interface ITelemetrySender
{
    /// <summary>
    /// Envia assincronamente o payload estruturado contendo os detalhes do erro crítico.
    /// </summary>
    /// <param name="payload">O objeto contendo os dados do erro, stack trace e contexto da requisição original.</param>
    /// <param name="cancellationToken">Token para monitorar e controlar o cancelamento cooperativo da operação assíncrona.</param>
    /// <returns>Uma Task representando a operação assíncrona de envio.</returns>
    Task SendAsync(ErrorTelemetryPayload payload, CancellationToken cancellationToken = default);
}