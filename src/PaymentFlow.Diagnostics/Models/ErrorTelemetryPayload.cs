namespace PaymentFlow.Diagnostics.Models;

/// <summary>
/// Representa o payload de dados de telemetria enviado para o webhook de observabilidade (n8n).
/// Contém os detalhes estruturados do incidente para diagnóstico via inteligência artificial.
/// </summary>
public class ErrorTelemetryPayload
{
    /// <summary>
    /// Nome do microsserviço ou aplicação de origem do erro (ex: "PaymentFlow.Api").
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Ambiente de execução onde ocorreu a falha (ex: "Production", "Development").
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Caminho do endpoint acessado no momento da requisição (ex: "/api/v1/payments/process").
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Verbo HTTP utilizado na requisição que gerou o erro (ex: "POST", "GET").
    /// </summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// Código de status HTTP resultante da falha (geralmente 500 - Internal Server Error).
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Tipo da classe de exceção disparada (ex: "System.NullReferenceException").
    /// </summary>
    public string ExceptionType { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem descritiva original lançada pela exceção.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Rastreamento completo da pilha de chamadas (Stack Trace) para análise da causa-raiz.
    /// </summary>
    public string StackTrace { get; set; } = string.Empty;

    /// <summary>
    /// Data e hora exata em que o incidente foi capturado (no formato universal UTC).
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}